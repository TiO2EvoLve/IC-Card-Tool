using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace D8_Demo.ViewModels;

public partial class ContentViewModel : ViewModelBase
{
    [DllImport("dcrf32.dll")]
    private static extern int dc_init(short port, int baud);
    [DllImport("dcrf32.dll")]
    private static extern short dc_beep(int icdev, uint _Msec);
    //端口号
    [ObservableProperty]private int port  = 100;
    //波特率
    [ObservableProperty]private int hz = 115200;
    //状态指示
    [ObservableProperty] private IImmutableSolidColorBrush color  = Brushes.Red;
    //波特率选项列表
    public ObservableCollection<string> Options { get;set;  } = ["1200", "2400", "4800","9600", "14400", "19200","28800", "38400", "57600", "115200","128000","230400","256000","460800","921600"];
    //选中的波特率
    [ObservableProperty] private string? selectedOption = "115200";
    //返回值
    private int icdev;
    [RelayCommand]
    private void OpenPort()
    {
        try
        {
            icdev = dc_init(Convert.ToInt16(Port), 115200);
            if (icdev < 0)
            {
                Console.WriteLine("失败", "打开端口失败");
            }
            else
            {
                Console.WriteLine("成功");
                Color = Brushes.LimeGreen;
                //dc_beep(icdev, 10);//蜂鸣
            }
        }
        catch (DllNotFoundException)
        {
            Console.WriteLine("找不到dcrf32.dll");
        }
    }
}