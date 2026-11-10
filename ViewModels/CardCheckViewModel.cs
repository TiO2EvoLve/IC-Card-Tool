using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media;
using D8_Demo.Enum;
using D8_Demo.Models;
using D8_Demo.Service;
using D8_Demo.Tool;
using MsBox.Avalonia;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace D8_Demo.ViewModels;

public partial class CardCheckViewModel : ReactiveObject
{
    private readonly CardHelper _cardHelper;
    private readonly ContentViewModel _contentViewModel;
    private readonly SettingsManage _settingsManage;
    private readonly CardErrorService _cardErrorService;
    private CancellationTokenSource? _cts;
    private Task? _runningTask;
    private ulong _lastAuthUid;

    [Reactive] private IImmutableSolidColorBrush _findCard = Brushes.Red;
    [Reactive] private string? _returnValue;
    [Reactive] private string? _cardType = "未检测到卡片";
    [Reactive] private string? _ATS = string.Empty;
    [Reactive] private string? _ATQA = string.Empty;
    [Reactive] private string? _SAK = string.Empty;
    [Reactive] private int _lastDuration;
    [Reactive] private int _count;
    [Reactive] private CardCheckType _cardCheckType = CardCheckType.Unknown;
    //是否进行外部认证
    [Reactive] private bool _isAuth;
    //认证是否成功
    [Reactive] private bool _authCheck;
    //可尝试次数
    [Reactive] private string _tryCount = string.Empty;
    //返回值对应的响应信息
    [Reactive] private string _respon = string.Empty;
    //标识是否正在检测中
    [Reactive] private bool _isChecking;

    //表示当前检测的卡片类型
    public bool IsCpu => CardCheckType == CardCheckType.Cpu;
    public bool IsM1 => CardCheckType == CardCheckType.M1;
    public bool IsUl => CardCheckType == CardCheckType.Ul;

    private static readonly IReadOnlyDictionary<string, string> ChipType = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["331A1"] = "FM1280-331-A1", ["334A1"] = "FM1280-334-A1", ["169A3"] = "FM1280-169-A3",
        ["331E1"] = "FM1280-331-E1", ["169C1"] = "FM1280-169-C1", ["2164"] = "FM1280-2164",
        ["2165"] = "FM1280-2165", ["165C1"] = "FM1280-165-C1", ["102A1"] = "FM1280-102-A1",
        ["137C3"] = "FM1280-137-C3", ["160110A1"] = "FM1216-110-A1", ["160109A1"] = "FM1216-109-A1"
    };

    public ObservableCollection<CardTable> Tables { get; } =
    [
        new() { Name = "S50", Atqa = "4", Sak = "08" }, new() { Name = "S70", Atqa = "4", Sak = "18" },
        new() { Name = "UL", Atqa = "44", Sak = "00" }, new() { Name = "CPU", Atqa = "8", Sak = "20" }
    ];

    public CardCheckViewModel(CardHelper cardHelper, ContentViewModel contentViewModel, SettingsManage settingsManage, CardErrorService cardErrorService)
    {
        _cardHelper = cardHelper;
        _contentViewModel = contentViewModel;
        _settingsManage = settingsManage;
        _cardErrorService = cardErrorService;
        this.WhenAnyValue(x => x.CardCheckType).Subscribe(_ =>
        {
            this.RaisePropertyChanged(nameof(IsCpu));
            this.RaisePropertyChanged(nameof(IsM1));
            this.RaisePropertyChanged(nameof(IsUl));
        });
    }

    [ReactiveCommand]
    private async Task CheckChipType()
    {
        if (!_cardHelper.isConnected) { await ShowMessage("警告", "请先打开端口"); return; }
        if (_cts != null) return;
        _cts = new CancellationTokenSource();
        IsChecking = true;
        _runningTask = Task.Run(() => Check(_cts.Token));
    }

    [ReactiveCommand]
    private Task StopChipType() => StopAsync();

    public async Task StopAsync(TimeSpan? timeout = null)
    {
        var cts = _cts;
        if (cts == null) return;
        await cts.CancelAsync();
        try { await (_runningTask ?? Task.CompletedTask).WaitAsync(timeout ?? TimeSpan.FromSeconds(2), cts.Token); }
        catch (TimeoutException) { }
        finally
        {
            FindCard = Brushes.Red;
            Count = 0;
            Clear();
            CardType = "已停止检测";
        }
    }

    private async Task Check(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                var stopwatch = Stopwatch.StartNew();
                if (!_cardHelper.isConnected) break;
                if (!_cardHelper.Reset()) { Clear(); CardType = "未检测到卡片"; await Delay(token); continue; }
                var cardUid = _cardHelper.FindCard();
                if (cardUid == 0) { Clear(); CardType = "未检测到卡片"; await Delay(token); continue; }

                FindCard = Brushes.LimeGreen;
                ATQA = _cardHelper.Request();
                var uid = _cardHelper.Anticoll();
                if (!string.IsNullOrEmpty(uid)) SAK = _cardHelper.Select(uid);
                CardCheckType = SAK switch { "08" or "18" => CardCheckType.M1, "00" => CardCheckType.Ul, "20" => CardCheckType.Cpu, _ => CardCheckType.Unknown };
                var ats = _cardHelper.ResetHex();
                if (string.IsNullOrEmpty(ats)) M1OrUL(ATQA ?? string.Empty, SAK ?? string.Empty);
                else
                {
                    //防止外部认证多次导致锁卡
                    if (IsAuth && cardUid != _lastAuthUid) { _lastAuthUid = cardUid; AuthCheck = CheckAuth(); }
                    ATS = ats;
                    _cardHelper.APDU("00A404000C54656D706F726172792E4D46");
                    var response = _cardHelper.APDU("80CA00F10A");
                    if (response == null) ReturnValue = "空";
                    else if (response.EndsWith("9000", StringComparison.Ordinal)) { ReturnValue = response; FM1280(response); }
                    else FM1208(ATQA ?? string.Empty, ATS ?? string.Empty);
                }
                FindCard = Brushes.Red;
                _cardHelper.Beep(_settingsManage.Settings.Sound);
                Count++;
                stopwatch.Stop();
                LastDuration = (int)stopwatch.ElapsedMilliseconds;
                await Delay(token);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { _contentViewModel.AddLog("卡片检测错误：" + ex.Message); }
        finally
        {
            FindCard = Brushes.Red;
            IsChecking = false;
            _cts?.Dispose(); _cts = null; _runningTask = null;
        }
    }

    private Task Delay(CancellationToken token) => Task.Delay(_settingsManage.Settings.SleepTime, token);
    private void FM1280(string response)
    {
        foreach (var entry in ChipType)
            if (response.Contains(entry.Key, StringComparison.OrdinalIgnoreCase)) { CardType = "芯片型号为:" + entry.Value; return; }
        CardType = "未知型号芯片";
    }
    private void FM1208(string atqa, string ats)
    {
        if (string.IsNullOrEmpty(ats) || ats.Length < 16) { CardType = "无法解析 ATS"; return; }
        var isZero = ats.Substring(ats.Length - 16, 8) == "00000000";
        CardType = atqa switch { "8" => isZero ? "FM1208-09或FM1216-109" : "FM1208-59或FM1208-73或FM1216-143", "4" => isZero ? "FM1208-10或FM1216-110" : "FM1208-76", _ => "未知 FM1208 特征" };
        ReturnValue = "该芯片类型无返回值";
    }
    private void M1OrUL(string atqa, string sak)
    {
        CardType = atqa switch { "44" when sak == "00" => "UL卡", "4" when sak == "08" => "M1 S50卡", "4" when sak == "18" => "M1 S70卡", _ => "未知芯片型号" };
        ReturnValue = "该芯片类型无返回值";
        ATS = "该芯片类型无ATS";
    }
    private void Clear()
    {
        FindCard = Brushes.Red; ReturnValue = ATS = ATQA = SAK = string.Empty; CardType = string.Empty;
        CardCheckType = CardCheckType.Unknown; AuthCheck = false; TryCount = Respon = string.Empty;
    }
    private Task ShowMessage(string title, string message) => MessageBoxManager.GetMessageBoxStandard(title, message).ShowAsync();
    private void AddLog(string msg) => _contentViewModel.AddLog(msg);
    private bool CheckAuth()
    {
        AddLog("进行外部认证验证");
        var random = _cardHelper.APDU("0084000008");
        if (string.IsNullOrWhiteSpace(random) || random.Length < 16) return false;
        var randomBytes = Tools.HexStringToBytes(random[..16]);
        var keyBytes = Tools.HexStringToBytes("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
        var data = Tools.EncryptEcb(randomBytes, keyBytes);
        var response = _cardHelper.APDU("0082000008" + BitConverter.ToString(data).Replace("-", string.Empty));
        if (response == "9000") { AddLog("外部认证成功"); return true; }
        if (response?.StartsWith("63C", StringComparison.Ordinal) == true) TryCount = response.Substring(3, 1);
        else { AddLog("外部认证返回值:" + response); Respon = _cardErrorService.GetMessage(response ?? string.Empty); }
        return false;
    }
}

