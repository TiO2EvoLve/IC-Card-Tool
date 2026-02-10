using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using D8_Demo.Tool;
using MsBox.Avalonia;

namespace D8_Demo.ViewModels;

public partial class CardCheckViewModel : ViewModelBase
{
    [ObservableProperty] private string? returnValue;//返回值
    [ObservableProperty] private string? cardType = "未检测到卡片";
    [ObservableProperty] private string? aTS = "";//ATS
    [ObservableProperty] private string? feature1 = "";//特征1
    [ObservableProperty] private string? feature2 = "";//特征2
    private readonly CardHelper CardHelper = CardHelper.Instance;
    private readonly ContentViewModel CVM = ContentViewModel.Instance;
    //读卡间隔
    public int ReadTime { get; set; } = 500;
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
        };
    private CancellationTokenSource? _cts;
    private Task? _runningTask;
    //TODO :发布时改为私有
    public CardCheckViewModel()
    {
    }
    //单例模式
    public static CardCheckViewModel Instance { get; } = new();

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

    private async Task Check(CancellationToken token)
    {
        AddLog("开始检测卡片...");
        while (!token.IsCancellationRequested)
        {
            if (!CardHelper.Reset() || CardHelper.FindCard() == 0)
            {
                AddLog("复位/寻卡失败");
                Clear();
                CardType = "未检测到卡片";
                await Task.Delay(ReadTime, token);
                continue;
            }
            //复位ATS
            var ats = CardHelper.ResetHex();
            if (ats == "")
            {
                AddLog("复位ATS失败");
                AddLog("卡片可能为M1卡或UL卡");
                //为M1卡或UL卡
                CardType = "M1卡或UL卡";
                await Task.Delay(ReadTime, token);
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
            //获取特征值1
            Feature1 = CardHelper.Request();
            //获取特征值2
            var uid =  CardHelper.Anticoll();
            if(uid != "") Feature2 = CardHelper.Select(uid);
            if (response.EndsWith("9000"))
            {
                AddLog("检测到 FM1280 系列芯片");
                ReturnValue = response;
                //芯片为1280
                FM1280(response);
            }else
            {
                //芯片为1208
                AddLog("检测到 FM12081 系列芯片");
                FM1208(Feature1,ATS);
            }
            await Task.Delay(ReadTime, token);
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

    private void FM1208(string feature, string ats)
    {
        if (string.IsNullOrEmpty(ats) || ats.Length < 16)
        {
            _ = Dispatcher.UIThread.InvokeAsync(() => CardType = "无法解析 ATS");
            return;
        }

        var core = ats.Substring(ats.Length - 16, 8); // safe now
        bool isZero = core == "00000000";
        switch (feature)
        {
            case "8":
                _ = Dispatcher.UIThread.InvokeAsync(() =>
                    CardType = isZero ? "FM1208-09或FM1216-109" : "FM1208-59或FM1208-73或FM1216-143");
                break;
            case "4":
                _ = Dispatcher.UIThread.InvokeAsync(() => CardType = isZero ? "FM1208-10或FM1216-110" : "FM1208-76");
                break;
            default:
                _ = Dispatcher.UIThread.InvokeAsync(() => CardType = "未知 FM1208 特征");
                break;
        }
    }

    void Clear()
    {
        ReturnValue = "";
        ATS = "";
        CardType = "";
        Feature1 = "";
        Feature2 = "";
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