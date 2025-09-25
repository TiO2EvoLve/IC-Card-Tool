using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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

public partial class ContentViewModel : ViewModelBase
{
    #region 导入DLL
    [DllImport("dcrf32.dll")] private static extern int dc_init(short port, int baud);//打开端口
    [DllImport("dcrf32.dll")] private static extern int dc_exit(int icdev);//关闭端口
    [DllImport("dcrf32.dll")] private static extern short dc_getname(int icdev, StringBuilder name);//获取设备名
    [DllImport("dcrf32.dll")] private static extern short dc_getver(int icdev, StringBuilder sver);//获取设备名
    [DllImport("dcrf32.dll")] private static extern short dc_beep(int icdev, uint _Msec);//蜂鸣
    [DllImport("dcrf32.dll")] private static extern int dc_card(int icdev, int mode, ref ulong snr);// 寻卡
    [DllImport("dcrf32.dll")] private static extern short dc_pro_resethex(int icdev, ref byte rlen, ref byte rbuff);//复位
    [DllImport("dcrf32.dll")] private static extern short dc_request(int icdev, int mode, ref short TagType);
    [DllImport("dcrf32.dll")] private static extern short dc_reset(int icdev, int Msec);
    #endregion
    //端口号
    [ObservableProperty] private int port = 100;
    //波特率
    [ObservableProperty] private int hz = 115200;
    //端口开启状态指示
    [ObservableProperty] private IImmutableSolidColorBrush color = Brushes.Red;
    //找到卡片状态指示
    [ObservableProperty] private IImmutableSolidColorBrush findCard =  Brushes.Red;
    //芯片基本信息
    [ObservableProperty] private string? uid16;
    [ObservableProperty] private string? uid16_;
    [ObservableProperty] private string? uid10;
    [ObservableProperty] private string? uid10_;
    [ObservableProperty] private string? ats;
    //设备名
    [ObservableProperty] private string? deviceName;
    [ObservableProperty] private string? firmwareVersion;
    //当前状态
    [ObservableProperty] private State? status = State.等待读取;
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
    private bool isConnected ;
    //卡号
    [ObservableProperty]private long sn;
    
    [ObservableProperty] private bool isChecked = true;
    //卡片列表
    public ObservableCollection<Card> Cards { get; set; } = new();
    //读卡间隔
    public int ReadTime;
    //是否蜂鸣
    public bool BeepSound = true;
    //TODO : 发布时改为私有
    public ContentViewModel() { }
    public static ContentViewModel Instance { get; } = new ();
    #region 打开端口
    [RelayCommand]
    public async Task OpenPort()
    {
        if (isConnected) return;//端口已经打开，无需继续操作。
        try
        {
            icdev = dc_init(Convert.ToInt16(Port), 115200);
            if (icdev < 0) {
                await MessageBoxManager.GetMessageBoxStandard("失败", "打开端口失败,请检查读卡器是否已连接").ShowAsync();
            }else {
                Color = Brushes.LimeGreen;
                isConnected = true;//记录端口打开成功
                if(BeepSound) dc_beep(icdev, 10);//蜂鸣
            }
        }catch (DllNotFoundException)
        {
            await MessageBoxManager.GetMessageBoxStandard("失败", "找不到dcrf32.dll，请检查").ShowAsync();
        }
        
        GetContent();
        

    }
    #endregion
    #region 获取设备信息
    private void GetContent()
    {
        //获取设备名
        StringBuilder name = new StringBuilder(64); // 分配64字节缓冲区
        if (dc_getname(icdev, name) == 0)
        {
            DeviceName = name.ToString();
        }
        //获取版本号
        StringBuilder ver = new StringBuilder(128); // 分配128字节缓冲
        if (dc_getver(icdev, ver) == 0)
        {
            FirmwareVersion = ver.ToString();
        }
    }
    #endregion
    #region 关闭端口
    [RelayCommand]
    public async Task ClosePort()
    {
        if (dc_exit(icdev) == 0)
        {
            Color = Brushes.Red;
            isConnected = false;
            if(BeepSound) dc_beep(icdev, 10);//蜂鸣
        }else
        {
            await MessageBoxManager.GetMessageBoxStandard("失败", "关闭端口失败").ShowAsync();
        }
    }
    #endregion 
    #region 读卡
    [RelayCommand]
    private async Task ReadCard()
    {
        if (!isConnected) await MessageBoxManager.GetMessageBoxStandard("警告", "请先打开端口").ShowAsync();
        await Task.Run(Read);
    }
    private void Read()
    {
        ulong uid = 0;//记录上次读的卡片
        while (true)
        {
            if (!isConnected) break; // 如果端口关闭，跳出循环
            if (dc_reset(icdev, 2) != 0)
            {
                Status = State.复位异常;
                return;
            }
            ulong CardUid = 0;
            if (dc_card(icdev, 0x01, ref CardUid) == 0) // 0 表示成功
            {
                FindCard = Brushes.LimeGreen;
                Status = State.读卡中;
                Uid10_ = CardUid.ToString();
                Uid16_ = CardUid.ToString("X");
                Uid16 = Tools.ChangeHexPairs(Uid16_);
                Uid10 = Convert.ToUInt32(Uid16, 16).ToString();
            }else
            {
                FindCard = Brushes.Red;
                Uid10 = "";
                Uid10_ = "";
                Uid16 = "";
                Uid16_ = "";
                Ats = "";
                Status = State.等待放卡;
                continue;
            }
            //取ATS
            byte crlen = 1;
            var recbuff = new byte[100];
            if (dc_pro_resethex(icdev, ref crlen, ref recbuff[0]) != 0)
            {
                Status = State.非CPU卡;
                Ats = "非CPU卡";
                Thread.Sleep(500);
                continue;
            }
            string num = "";
            foreach (var t in recbuff)
            {
                num += (char)t;
            }
            num = num.Replace("\0", "");
            Ats = num;
            //判断是否为同一张卡
            if (uid == CardUid)
            {
                Status = State.同一张卡;
                Thread.Sleep(500);
                continue;
            }else if(BeepSound) 
            { 
                dc_beep(icdev, 10);//蜂鸣
            }
            uid = CardUid;
            //判断是否已经有该数据
            if (Cards != null && Cards.All(card => card.UID10_ != Uid10_))
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (IsChecked)
                    {
                        Cards.Add(new Card
                        {
                            SN = Sn,
                            UID10 = Uid10,
                            UID10_ = Uid10_,
                            UID16 = Uid16,
                            UID16_ = Uid16_,
                            ATS = Ats,
                            Time = DateTime.Now
                        });
                        Sn++;
                    }
                    else
                    {
                        Cards.Add(new Card
                        {
                            UID10 = Uid10,
                            UID10_ = Uid10_,
                            UID16 = Uid16,
                            UID16_ = Uid16_,
                            ATS = Ats,
                            Time = DateTime.Now
                        }); 
                    }
                    
                });
            }
            Thread.Sleep(ReadTime);
        }
    }
    #endregion
    #region 清除列表
    [RelayCommand]
    private void ClearCards()
    {
        Cards.Clear();
    }
    #endregion
    #region 保存数据
    [RelayCommand]
    private async Task SaveData()
    {
        if (Cards.Count == 0)
        {
            await MessageBoxManager.GetMessageBoxStandard("失败", "没有数据需要保存").ShowAsync();
            return;
        }
        var sourceFilePath = "temple/卡片信息.mdb";
        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var destinationFilePath = Path.Combine(desktopPath, "卡片信息.mdb");
        try
        {
            await Task.Run(() => File.Copy(sourceFilePath, destinationFilePath, true));
        }
        catch (Exception ex)
        {
            await MessageBoxManager.GetMessageBoxStandard("异常", ex.Message).ShowAsync();
            return;
        }
        //保存数据到数据库
        var sqls = new List<string>();

        // 先清空表
        sqls.Add("DELETE FROM kahao");
        
        foreach (var card in Cards)
        {
            string sql = $@"
        INSERT INTO kahao (SN, ATS, UID16, UID16_, UID10, UID10_, [Time]) 
        VALUES ('{card.SN}', '{card.ATS}', '{card.UID16}', '{card.UID16_}', '{card.UID10}', '{card.UID10_}', #{card.Time:yyyy-MM-dd HH:mm:ss}#)";
            sqls.Add(sql);
        }
        try
        {
            await Task.Run(() => Mdb.ExecuteBatch(destinationFilePath, sqls));
            await MessageBoxManager.GetMessageBoxStandard("成功", "数据已保存到桌面").ShowAsync();
        }
        catch (Exception ex)
        {
            await MessageBoxManager.GetMessageBoxStandard("失败", $"保存失败：{ex.Message}").ShowAsync();
        }
    }
    #endregion
}
   