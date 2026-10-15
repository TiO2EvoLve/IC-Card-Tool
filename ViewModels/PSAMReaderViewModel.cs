using System;
using System.Threading.Tasks;
using System.Threading;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using D8_Demo.Tool;
using MsBox.Avalonia;

namespace D8_Demo.ViewModels;

public partial class PSAMReaderViewModel : ViewModelBase
{
    //端口号
    [ObservableProperty] private int port = 100;

    //波特率
    [ObservableProperty] private int hz = 115200;
    [ObservableProperty] private string psamData = "0084000010";

    [ObservableProperty] private string resData = "";

    //卡片帮助类
    private readonly CardHelper CardHelper;

    public PSAMReaderViewModel(CardHelper cardHelper)
    {
        CardHelper = cardHelper;
    }

    private CancellationTokenSource? _cts;
    private Task? _runningTask;

    [RelayCommand]
    private void ReadPSAM()
    {
        // run synchronously because it's short, but expose lifecycle methods
        Console.WriteLine(CardHelper.Setcpu() ? "PSAM设置卡座成功" : "设置卡座失败");
        Console.WriteLine(CardHelper.PSAMReset() ? "PSAM复位成功" : "复位失败");
        string response = CardHelper.PSAMAPDU(PsamData);
        ResData = String.IsNullOrEmpty(response) ? "APDU指令发送失败" : response;
    }

    public Task StartAsync()
    {
        // no continuous loop by default; no-op but present for compatibility
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        // no continuous loop to stop, keep compatibility
        return Task.CompletedTask;
    }

    // 显示消息框
    async Task ShowMessage(string title, string message)
    {
        await MessageBoxManager.GetMessageBoxStandard(title, message).ShowAsync();
    }

    [RelayCommand]
    private void Test()
    {
        if (!CardHelper.Reset() || CardHelper.FindCard() == 0)
        {
            Console.WriteLine("寻卡失败");
        }

        //复位ATS
        if (CardHelper.ResetHex() == "")
        {
            Console.WriteLine("复位HEX失败");
        }

        var response = CardHelper.APDU("0084000008");
        Console.WriteLine(
            "随机数为                                                                                                                                                                         " +
            response);
    }
}