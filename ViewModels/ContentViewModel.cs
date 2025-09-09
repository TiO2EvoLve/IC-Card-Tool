using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using D8_Demo.Views;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace D8_Demo.ViewModels;

public partial class ContentViewModel : ViewModelBase
{
    [DllImport("dcrf32.dll")] private static extern int dc_init(short port, int baud);//打开端口
    [DllImport("dcrf32.dll")] private static extern int dc_exit(int icdev);//关闭端口
    [DllImport("dcrf32.dll")] private static extern short dc_beep(int icdev, uint _Msec);//蜂鸣
    
    [DllImport("dcrf32.dll")] private static extern int dc_card(int icdev, int mode, ref ulong snr);// 寻卡
    [DllImport("dcrf32.dll")] private static extern short dc_pro_resethex(int icdev, ref byte rlen, ref byte rbuff);
    [DllImport("dcrf32.dll")] private static extern short dc_request(int icdev, int mode, ref short TagType);
    //端口号
    [ObservableProperty] private int port = 100;
    //波特率
    [ObservableProperty] private int hz = 115200;
    //状态指示
    [ObservableProperty] private IImmutableSolidColorBrush color = Brushes.Red;
    //芯片基本信息
    [ObservableProperty] private string? uid16;
    [ObservableProperty] private string? uid16_;
    [ObservableProperty] private string? uid10;
    [ObservableProperty] private string? uid10_;
    [ObservableProperty] private string? ats;

    //波特率选项列表
    public ObservableCollection<string> Options { get; set; } =
    [
        "1200", "2400", "4800", "9600", "14400", "19200", "28800", "38400", "57600", "115200", "128000", "230400",
        "256000", "460800", "921600"
    ];
    //选中的波特率
    [ObservableProperty] private string? selectedOption = "115200";
    //设备标识符
    private int icdev;
    //是否已连接
    private bool isConnected = false;
    //返回值
    private int st = 1;

    //打开端口
    [RelayCommand]
    private async Task OpenPort()
    {
        if (isConnected) return;//端口已经打开，无需继续操作。
        try
        {
            icdev = dc_init(Convert.ToInt16(Port), 115200);
            if (icdev < 0) {
                await MessageBoxManager.GetMessageBoxStandard("失败", "打开端口失败,请检查读卡器是否已链接").ShowAsync();
            }else {
                Color = Brushes.LimeGreen;
                isConnected = true;//记录端口打开成功
                //dc_beep(icdev, 10);//蜂鸣
            }
        }
        catch (DllNotFoundException)
        {
            await MessageBoxManager.GetMessageBoxStandard("失败", "找不到dcrf32.dll，请检查").ShowAsync();
        }
    }
    //关闭端口
    [RelayCommand]
    private async Task ClosePort()
    {
        if (dc_exit(icdev) == 0)
        {
            Color = Brushes.Red;
            isConnected = false;
        }else
        {
            await MessageBoxManager.GetMessageBoxStandard("失败", "关闭端口失败").ShowAsync();
        }
        
    }
    //读卡
    [RelayCommand]
    private async Task ReadCard()
    {
        if (!isConnected) await MessageBoxManager.GetMessageBoxStandard("警告", "请先打开端口").ShowAsync();
        while (true)
        {
            // ulong cardno = 0;
            // if (dc_card(icdev, 1, ref cardno) == 0) // 0 表示成功
            // {
            //     Console.WriteLine("检测到卡片，卡号: " + cardno);
            //     Thread.Sleep(1000); // 延时避免重复读取
            // }
            short TagType = 0;
            int st = dc_request(icdev,0,ref TagType);
            Console.WriteLine(st);
            //Console.WriteLine(TagType);
            Thread.Sleep(2000);
        }
    }
}
   