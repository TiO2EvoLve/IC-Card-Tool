using System;
using System.Threading.Tasks;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using D8_Demo.Tool;
using MsBox.Avalonia;

namespace D8_Demo.ViewModels;

public partial class PSAMReaderViewModel: ViewModelBase
{
    //端口号
    [ObservableProperty] private int port = 100;
    //波特率
    [ObservableProperty] private int hz = 115200;
    [ObservableProperty] private string psamData = "0084000010";
    [ObservableProperty] private string resData = "";
    //卡片帮助类
    private readonly CardHelper CardHelper = CardHelper.Instance;
    public static PSAMReaderViewModel Instance { get; } = new();
    //TODO :发布时改为私有
    public PSAMReaderViewModel() { }
    
    [RelayCommand]
    private void ReadPSAM()
    {
        Console.WriteLine(CardHelper.Setcpu() ? "PSAM设置卡座成功" : "设置卡座失败");
        Console.WriteLine(CardHelper.PSAMReset() ? "PSAM复位成功" : "复位失败");
        string response = CardHelper.PSAMAPDU(PsamData);
        ResData = String.IsNullOrEmpty(response) ? "APDU指令发送失败" : response;
    }
    // 显示消息框
    async Task ShowMessage(string title, string message)
    {
        await MessageBoxManager.GetMessageBoxStandard(title, message).ShowAsync();
    }
}