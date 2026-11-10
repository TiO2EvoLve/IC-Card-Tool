using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using D8_Demo.Enum;
using D8_Demo.Models;
using D8_Demo.Tool;
using MsBox.Avalonia;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Stateless;

namespace D8_Demo.ViewModels;

public sealed partial class ContentViewModel : ReactiveObject
{
    private enum ReaderState { PortClosed, PortOpen, Reading }
    private enum ReaderTrigger { OpenSucceeded, CloseSucceeded, StartReading, StopReading, PortLost }

    private readonly CardHelper _cardHelper;
    private readonly AppSettings _settings;
    private readonly Mdb _mdb;
    private readonly StateMachine<ReaderState, ReaderTrigger> _machine;
    private CancellationTokenSource? _cts;
    private Task? _runningTask;

    [Reactive] private int _port = 100;
    [Reactive] private int _hz = 115200;
    [Reactive(SetModifier = AccessModifier.Private)] private IImmutableSolidColorBrush _color = Brushes.Red;
    [Reactive(SetModifier = AccessModifier.Private)] private IImmutableSolidColorBrush _findCard = Brushes.Red;
    [Reactive(SetModifier = AccessModifier.Private)] private string? _uid16, _uid16_, _uid10, _uid10_, _ats, _deviceName, _firmwareVersion;
    [Reactive(SetModifier = AccessModifier.Private)] private State? _status = State.等待读取;    public ObservableCollection<string> Options { get; } = ["1200", "2400", "4800", "9600", "14400", "19200", "28800", "38400", "57600", "115200", "128000", "230400", "256000", "460800", "921600"];
    [Reactive] private string? _selectedOption = "115200";
    [Reactive] private string _sn = "1";
    [Reactive] private bool _isChecked = true;
    public ObservableCollection<Card> Cards { get; } = new();
    [Reactive(SetModifier = AccessModifier.Private)] private bool _isRunning;
    [Reactive(SetModifier = AccessModifier.Private)] private TextDocument _logDoc = new();
    public ReactiveCommand<ReactiveUI.Primitives.RxVoid, ReactiveUI.Primitives.RxVoid> OpenPortCommand { get; }
    public ReactiveCommand<ReactiveUI.Primitives.RxVoid, ReactiveUI.Primitives.RxVoid> ClosePortCommand { get; }
    public ReactiveCommand<ReactiveUI.Primitives.RxVoid, ReactiveUI.Primitives.RxVoid> ReadCardCommand { get; }
    public ReactiveCommand<ReactiveUI.Primitives.RxVoid, ReactiveUI.Primitives.RxVoid> ClearCardsCommand { get; }
    public ReactiveCommand<ReactiveUI.Primitives.RxVoid, ReactiveUI.Primitives.RxVoid> SaveDataCommand { get; }
    public ReactiveCommand<ReactiveUI.Primitives.RxVoid, ReactiveUI.Primitives.RxVoid> ClearLogsCommand { get; }

    public ContentViewModel(CardHelper cardHelper, SettingsManage settingsManage, Mdb mdb)
    {
        _cardHelper = cardHelper;
        _settings = settingsManage.Settings;
        _mdb = mdb;
        _machine = new StateMachine<ReaderState, ReaderTrigger>(cardHelper.isConnected ? ReaderState.PortOpen : ReaderState.PortClosed);
        _machine.Configure(ReaderState.PortClosed).Permit(ReaderTrigger.OpenSucceeded, ReaderState.PortOpen);
        _machine.Configure(ReaderState.PortOpen).Permit(ReaderTrigger.CloseSucceeded, ReaderState.PortClosed).Permit(ReaderTrigger.StartReading, ReaderState.Reading);
        _machine.Configure(ReaderState.Reading).Permit(ReaderTrigger.StopReading, ReaderState.PortOpen).Permit(ReaderTrigger.PortLost, ReaderState.PortClosed);
        OpenPortCommand = ReactiveCommand.CreateFromTask(OpenPort);
        ClosePortCommand = ReactiveCommand.CreateFromTask(ClosePort);
        ReadCardCommand = ReactiveCommand.CreateFromTask(ReadCard);
        ClearCardsCommand = ReactiveCommand.Create(ClearCards);
        SaveDataCommand = ReactiveCommand.CreateFromTask(SaveData);
        ClearLogsCommand = ReactiveCommand.Create(ClearLogs);
    }

    public async Task OpenPort()
    {
        if (_machine.State != ReaderState.PortClosed) return;
        try
        {
            if (!_cardHelper.OpenPort(Convert.ToInt16(Port), Convert.ToInt32(SelectedOption))) { await ShowMessage("失败", "打开端口失败,请检查读卡器是否已连接"); return; }
            _machine.Fire(ReaderTrigger.OpenSucceeded);
            Color = Brushes.LimeGreen;
            _cardHelper.Beep(_settings.Sound);
            AddLog("打开端口成功！");
        }
        catch (DllNotFoundException) { await ShowMessage("失败", "请根据你的系统更换对应的dcrf32.dll，请检查"); }
        DeviceName = _cardHelper.GetDeviceName();
        FirmwareVersion = _cardHelper.GetDeviceVersion();
    }

    public async Task ClosePort()
    {
        if (IsRunning) await StopAsync();
        if (_cardHelper.ClosePort())
        {
            if (_machine.State == ReaderState.PortOpen) _machine.Fire(ReaderTrigger.CloseSucceeded);
            Color = Brushes.Red;
            AddLog("端口已关闭");
        }
        else await MessageBoxManager.GetMessageBoxStandard("失败", "关闭端口失败").ShowAsync();
    }

    private async Task ReadCard()
    {
        if (!_cardHelper.isConnected || _machine.State != ReaderState.PortOpen) { await ShowMessage("警告", "请先打开端口"); return; }
        _machine.Fire(ReaderTrigger.StartReading);
        _cts = new CancellationTokenSource();
        IsRunning = true;
        _runningTask = Task.Run(() => RunLoopAsync(_cts.Token));
    }

    public async Task StopAsync(TimeSpan? timeout = null)
    {
        var cts = _cts;
        if (cts == null) return;
        await cts.CancelAsync();
        try { await (_runningTask ?? Task.CompletedTask).WaitAsync(timeout ?? TimeSpan.FromSeconds(2)); }
        catch (TimeoutException) { }
        finally { _cardHelper.Beep(_settings.Sound); }
    }

    private async Task RunLoopAsync(CancellationToken token)
    {
        ulong previousUid = 0;
        try
        {
            while (!token.IsCancellationRequested)
            {
                if (!_cardHelper.isConnected) { await UiAsync(() => { if (_machine.State == ReaderState.Reading) _machine.Fire(ReaderTrigger.PortLost); }); break; }
                if (!_cardHelper.Reset())
                {
                    await UiAsync(() => { Status = State.复位异常; AddLog("复位异常"); });
                    break;
                }
                var cardUid = _cardHelper.FindCard();
                if (cardUid == 0) { await UiAsync(Clear); await Task.Delay(_settings.SleepTime, token); continue; }
                var uid16Raw = cardUid.ToString("X").PadLeft(8, '0');
                var uid16 = Tools.ChangeHexPairs(uid16Raw).PadLeft(8, '0');
                var uid10 = Convert.ToUInt32(uid16, 16).ToString();
                var uid10Raw = Convert.ToUInt32(uid16Raw, 16).ToString();
                var ats = _cardHelper.ResetHex();
                await UiAsync(() =>
                {
                    FindCard = Brushes.LimeGreen; Uid16_ = uid16Raw; Uid16 = uid16; Uid10 = uid10; Uid10_ = uid10Raw;
                    AddLog($"发现卡片:{uid16}"); Status = string.IsNullOrEmpty(ats) ? State.非CPU卡 : State.读卡中;
                    Ats = string.IsNullOrEmpty(ats) ? "非CPU卡" : ats;
                    if (previousUid == cardUid) Status = State.同一张卡;
                    if (Cards.All(card => card.UID10_ != uid10Raw))
                    {
                        var item = new Card { UID10 = uid10, UID10_ = uid10Raw, UID16 = uid16, UID16_ = uid16Raw, ATS = Ats, Time = DateTime.Now };
                        if (IsChecked) { item.SN = Sn; Sn = (Convert.ToInt32(Sn) + 1).ToString(); }
                        Cards.Add(item); _cardHelper.Beep(_settings.Sound);
                    }
                });
                previousUid = cardUid;
                if (previousUid == cardUid) await Task.Delay(500, token);
                await Task.Delay(_settings.SleepTime, token);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { await UiAsync(() => AddLog("读取错误：" + ex.Message)); }
        finally
        {
            await UiAsync(() => { FindCard = Brushes.Red; Status = State.等待读取; IsRunning = false; if (_machine.State == ReaderState.Reading) _machine.Fire(ReaderTrigger.StopReading); });
            _cts?.Dispose(); _cts = null; _runningTask = null;
        }
    }

    private static Task UiAsync(Action action) => Dispatcher.UIThread.InvokeAsync(action).GetTask();
    private void ClearCards() => Cards.Clear();
    private async Task SaveData()
    {
        if (Cards.Count == 0) { await ShowMessage("失败", "没有数据需要保存"); return; }
        var destination = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "卡片信息.mdb");
        try { await Task.Run(() => File.Copy("temple/卡片信息.mdb", destination, true)); }
        catch (Exception ex) { await ShowMessage("失败", $"保存失败：{ex.Message}"); return; }
        var sqls = new List<string> { "DELETE FROM kahao" };
        sqls.AddRange(Cards.Select(card => $@"INSERT INTO kahao (SN, ATS, UID16, UID16_, UID10, UID10_, [Time]) VALUES ('{card.SN}', '{card.ATS}', '{card.UID16}', '{card.UID16_}', '{card.UID10}', '{card.UID10_}', #{card.Time:yyyy-MM-dd HH:mm:ss}#)"));
        try { await Task.Run(() => _mdb.ExecuteBatch(destination, sqls)); await ShowMessage("成功", "数据已保存到桌面"); }
        catch (Exception ex) { await ShowMessage("失败", $"保存失败：{ex.Message}"); }
    }
    private void Clear() { FindCard = Brushes.Red; Uid10 = Uid10_ = Uid16 = Uid16_ = Ats = ""; Status = State.等待放卡; }
    private void ClearLogs() => LogDoc.Text = string.Empty;
    public void AddLog(string msg) => Dispatcher.UIThread.Post(() => LogDoc.Insert(LogDoc.TextLength, $"{msg}{Environment.NewLine}"));
    private static Task ShowMessage(string title, string message) => MessageBoxManager.GetMessageBoxStandard(title, message).ShowAsync();
}


