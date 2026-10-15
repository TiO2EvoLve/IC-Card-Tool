using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using D8_Demo.Enum;
using D8_Demo.Models;
using D8_Demo.Tool;
using MsBox.Avalonia;

namespace D8_Demo.ViewModels;

public partial class CardCheckViewModel : ViewModelBase
{
    [ObservableProperty] private IImmutableSolidColorBrush findCard = Brushes.Red;
    [ObservableProperty] private string? returnValue; //返回值
    [ObservableProperty] private string? cardType = "未检测到卡片";
    [ObservableProperty] private string? aTS = ""; //ATS
    [ObservableProperty] private string? aTQA = ""; //ATQA
    [ObservableProperty] private string? sAK = ""; //SAK
    [ObservableProperty] private int lastDuration; //上次耗时
    [ObservableProperty] private int count; //计数

    private readonly CardHelper CardHelper;
    private readonly ContentViewModel CVM;
    private readonly AppSettings Settings;

    private static readonly IReadOnlyDictionary<string, string> ChipType =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "331A1", "FM1280-331-A1" },
            { "334A1", "FM1280-334-A1" },
            { "169A3", "FM1280-169-A3" },
            { "331E1", "FM1280-331-E1" },
            { "169C1", "FM1280-169-C1" },
            { "2164", "FM1280-2164" },
            { "2165", "FM1280-2165" },
            { "165C1", "FM1280-165-C1" },
            { "102A1", "FM1280-102-A1" },
            { "137C3", "FM1280-137-C3" },
        };

    // 卡片对照表
    public ObservableCollection<CardTable> Tables { get; } = new()
    {
        new CardTable { Name = "S50", Atqa = "4", Sak = "08" },
        new CardTable { Name = "S70", Atqa = "4", Sak = "18" },
        new CardTable { Name = "UL", Atqa = "44", Sak = "00" },
        new CardTable { Name = "CPU", Atqa = "8", Sak = "20" },
    };

    private CancellationTokenSource? _cts;
    private Task? _runningTask;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCpu))]
    [NotifyPropertyChangedFor(nameof(IsM1))]
    [NotifyPropertyChangedFor(nameof(IsUl))]
    private CardCheckType cardCheckType = CardCheckType.Unknown;

    public bool IsCpu => CardCheckType == CardCheckType.Cpu;
    public bool IsM1 => CardCheckType == CardCheckType.M1;
    public bool IsUl => CardCheckType == CardCheckType.Ul;

    public CardCheckViewModel(
        CardHelper cardHelper,
        ContentViewModel contentViewModel,
        SettingsManage settingsManage)
    {
        CardHelper = cardHelper;
        CVM = contentViewModel;
        Settings = settingsManage.Settings;
    }

    private Task StartAsync()
    {
        if (_cts != null) return Task.CompletedTask;
        _cts = new CancellationTokenSource();
        _runningTask = Task.Run(() => Check(_cts.Token));
        return Task.CompletedTask;
    }

    public async Task StopAsync(TimeSpan? timeout = null)
    {
        if (_cts == null) return;
        await _cts.CancelAsync();
        timeout ??= TimeSpan.FromSeconds(2);
        try
        {
            await Task.WhenAny(_runningTask ?? Task.CompletedTask, Task.Delay(timeout.Value));
        }
        catch
        {
            // ignored
        }
        finally
        {
            FindCard = Brushes.Red;
            Count = 0; //计数归0
            CardType = "已停止检测";
        }

        ;
        _cts.Dispose();
        _cts = null;
    }

    [RelayCommand]
    private void CheckChipType()
    {
        if (!CardHelper.isConnected)
        {
            _ = ShowMessage("警告", "请先打开端口");
            return;
        }

        _ = StartAsync();
    }

    [RelayCommand]
    private void StopChipType()
    {
        _ = StopAsync();
    }

    private async Task Check(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            // 开始计时
            var stopwatch = Stopwatch.StartNew();
            FindCard = Brushes.LimeGreen;
            if (!CardHelper.Reset() || CardHelper.FindCard() == 0)
            {
                AddLog("复位/寻卡失败");
                Clear();
                CardType = "未检测到卡片";
                await Task.Delay(Settings.SleepTime, token);
                continue;
            }

            //获取ATQA
            ATQA = CardHelper.Request();
            //获取SAK
            var uid = CardHelper.Anticoll();
            if (uid != "") SAK = CardHelper.Select(uid);
            CardCheckType = SAK switch
            {
                "08" or "18" => CardCheckType.M1,
                "00" => CardCheckType.Ul,
                "20" => CardCheckType.Cpu,
                _ => CardCheckType.Unknown,
            };
            //复位ATS
            var ats = CardHelper.ResetHex();
            if (ats == "")
            {
                //继续判断是M1或UL卡
                M1OrUL(ATQA, SAK);
                Count++;
                // 停止计时并记录
                stopwatch.Stop();
                LastDuration = (int)stopwatch.ElapsedMilliseconds;
                await Task.Delay(Settings.SleepTime, token);
                continue;
            }

            ATS = ats;
            //选择主目录
            _ = CardHelper.APDU("00A404000C54656D706F726172792E4D46");
            var response = CardHelper.APDU("80CA00F10A");
            if (response == null)
            {
                ReturnValue = "空";
                continue;
            }

            if (response.EndsWith("9000"))
            {
                ReturnValue = response;
                //芯片为1280
                FM1280(response);
            }
            else
            {
                //芯片为1208
                FM1208(ATQA, ATS);
            }

            FindCard = Brushes.Red;
            CardHelper.Beep(Settings.Sound);
            Count++;
            // 停止计时并记录
            stopwatch.Stop();
            LastDuration = (int)stopwatch.ElapsedMilliseconds;
            await Task.Delay(Settings.SleepTime, token);
        }
    }

    private void FM1280(string response)
    {
        var result = "未知型号芯片";
        foreach (var kv in ChipType)
        {
            if (response.Contains(kv.Key, StringComparison.OrdinalIgnoreCase))
            {
                result = "芯片型号为:" + kv.Value;
                break;
            }
        }

        // 确保在 UI 线程更新属性
        _ = Dispatcher.UIThread.InvokeAsync(() => CardType = result);
    }

    private void FM1208(string atqa, string ats)
    {
        if (string.IsNullOrEmpty(ats) || ats.Length < 16)
        {
            _ = Dispatcher.UIThread.InvokeAsync(() => CardType = "无法解析 ATS");
            return;
        }

        var core = ats.Substring(ats.Length - 16, 8); // safe now
        var isZero = core == "00000000";
        _ = atqa switch
        {
            "8" => Dispatcher.UIThread.InvokeAsync(() =>
                CardType = isZero ? "FM1208-09或FM1216-109" : "FM1208-59或FM1208-73或FM1216-143"),
            "4" => Dispatcher.UIThread.InvokeAsync(() => CardType = isZero ? "FM1208-10或FM1216-110" : "FM1208-76"),
            _ => Dispatcher.UIThread.InvokeAsync(() => CardType = "未知 FM1208 特征")
        };
        ReturnValue = "该芯片类型无返回值";
    }

    private void M1OrUL(string atqa, string sak)
    {
        CardType = atqa switch
        {
            "44" when sak == "00" => "UL卡",
            "4" when (sak == "08") => "M1 S50卡",
            "4" when (sak == "18") => "M1 S70卡",
            _ => "未知芯片型号"
        };
        ReturnValue = "该芯片类型无返回值";
        ATS = "该芯片类型无ATS";
    }

    void Clear()
    {
        ReturnValue = "";
        ATS = "";
        CardType = "";
        ATQA = "";
        SAK = "";
        CardCheckType = CardCheckType.Unknown;
    }

    // 显示消息框
    private async Task ShowMessage(string title, string message)
    {
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await MessageBoxManager.GetMessageBoxStandard(title, message).ShowAsync();
        });
    }

    // 添加日志
    void AddLog(string msg)
    {
        CVM.AddLog(msg);
    }
}