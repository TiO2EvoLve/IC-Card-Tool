using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using D8_Demo.Enum;
using D8_Demo.Models;
using D8_Demo.Tool;
using MsBox.Avalonia;

namespace D8_Demo.ViewModels;

public partial class ContentViewModel : ViewModelBase
{
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
    //卡号
    [ObservableProperty]private string sn = "1";
    [ObservableProperty] private bool isChecked = true;
    //卡片列表
    public ObservableCollection<Card> Cards { get; set; } = new();
    //读卡间隔
    public int ReadTime;
    //是否蜂鸣
    public bool BeepSound = true;
    //运行状态
    [ObservableProperty] private bool isRunning;
    //日志内容
    [ObservableProperty] private TextDocument logDoc = new ();
    //卡片帮助类
    private readonly CardHelper CardHelper = CardHelper.Instance;
    private CancellationTokenSource? _cts;
    private Task? _runningTask;
    //TODO :发布时改为私有
    public ContentViewModel() { }
    public static ContentViewModel Instance { get; } = new ();
    #region 打开端口
    [RelayCommand]
    public async Task OpenPort()
    {
        if (CardHelper.isConnected) return; //端口已经打开，无需继续操作。
        try
        {
            if (!CardHelper.OpenPort(Convert.ToInt16(Port), Convert.ToInt32(SelectedOption)))
            {
                await ShowMessage("失败", "打开端口失败,请检查读卡器是否已连接");
            }else
            {
                Color = Brushes.LimeGreen;
                CardHelper.Beep(BeepSound);
                AddLog("打开端口成功！");
            }
        }
        catch (DllNotFoundException)
        {
            await ShowMessage("失败", "请根据你的系统更换对应的dcrf32.dll，请检查");
        }
        DeviceName = CardHelper.GetDeviceName();
        FirmwareVersion = CardHelper.GetDeviceVersion();

    }
    #endregion
    #region 关闭端口
    [RelayCommand]
    public async Task ClosePort()
    {
        if (CardHelper.ClosePort())
        {
            Color = Brushes.Red;
            AddLog("端口已关闭");
        }
        else
        {
            await MessageBoxManager.GetMessageBoxStandard("失败", "关闭端口失败").ShowAsync();
        }
    }
    #endregion 
    #region 读卡
    [RelayCommand]
    private async Task ReadCard()
    {
        if (!CardHelper.isConnected) await MessageBoxManager.GetMessageBoxStandard("警告", "请先打开端口").ShowAsync();
        await StartAsync();
    }

    private Task StartAsync()
    {
        if (_cts != null) return Task.CompletedTask; // already running
        _cts = new CancellationTokenSource();
        IsRunning = true;
        _runningTask = Task.Run(() => RunLoopAsync(_cts.Token));
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
            CardHelper.Beep(BeepSound);
        }

        _cts.Dispose();
        _cts = null;
        IsRunning = false;
    }

    private async Task RunLoopAsync(CancellationToken token)
    {
        ulong uid = 0;//记录上次读的卡片
        try
        {
            while (!token.IsCancellationRequested)
            {
                if (!CardHelper.isConnected) break; // 如果端口关闭，跳出循环
                if (!CardHelper.Reset())
                {
                    await Dispatcher.UIThread.InvokeAsync(() => {
                        Status = State.复位异常;
                        AddLog("复位异常");
                    });
                    return;
                }
                ulong CardUid = CardHelper.FindCard();
                if (CardUid != 0) // 0 表示失败
                {
                    await Dispatcher.UIThread.InvokeAsync(() => {
                        FindCard = Brushes.LimeGreen;
                        Status = State.读卡中;
                    });
                    
                    var uid16_ = CardUid.ToString("X");
                    var uid16 = Tools.ChangeHexPairs(uid16_);
                    var uid10 = Convert.ToUInt32(uid16, 16).ToString();

                    await Dispatcher.UIThread.InvokeAsync(() => {
                        Uid10_ = uid16_;
                        Uid16_ = uid16_;
                        Uid16 = uid16;
                        Uid10 = uid10;
                        AddLog($"发现卡片:{Uid16}");
                    });
                    
                }
                else
                {
                    await Dispatcher.UIThread.InvokeAsync(() => Clear());
                    await Task.Delay(ReadTime, token);
                    continue;
                }
                //取ATS
                Ats = CardHelper.ResetHex();
                if (string.IsNullOrEmpty(Ats))
                {
                    await Dispatcher.UIThread.InvokeAsync(() => {
                        Status = State.非CPU卡;
                        Ats = "非CPU卡";
                    });
                    await Task.Delay(500, token);
                    continue;
                }

                //判断是否为同一张卡
                if (uid == CardUid)
                {
                    await Dispatcher.UIThread.InvokeAsync(() => Status = State.同一张卡);
                    await Task.Delay(500, token);
                }
                uid = CardUid;
                
                //判断是否已经有该数据并添加
                if (Cards != null && Cards.All(card => card.UID10_ != Uid10_))
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
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
                            Sn = (Convert.ToInt32(Sn) + 1).ToString();
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
                CardHelper.Beep(BeepSound);
                await Task.Delay(ReadTime, token);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.InvokeAsync(() => AddLog("RunLoop 出错:" + ex.Message));
        }
        finally
        {
            await Dispatcher.UIThread.InvokeAsync(() => {
                FindCard = Brushes.Red;
                Status = State.等待读取;
                IsRunning = false;
            });
            _cts?.Dispose();
            _cts = null;
            _runningTask = null;
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
            await ShowMessage("失败", "没有数据需要保存");
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
            await ShowMessage("失败", $"保存失败：{ex.Message}");
            return;
        }
        //保存数据到数据库
        var sqls = new List<string> {
            // 先清空表
            "DELETE FROM kahao" };
        sqls.AddRange(Cards.Select(card => $@"
        INSERT INTO kahao (SN, ATS, UID16, UID16_, UID10, UID10_, [Time]) 
        VALUES ('{card.SN}', '{card.ATS}', '{card.UID16}', '{card.UID16_}', '{card.UID10}', '{card.UID10_}', #{card.Time:yyyy-MM-dd HH:mm:ss}#)"));
        try
        {
            await Task.Run(() => Mdb.ExecuteBatch(destinationFilePath, sqls));
            await ShowMessage("成功", "数据已保存到桌面");
        }
        catch (Exception ex)
        {
            await ShowMessage("失败", $"保存失败：{ex.Message}");
        }
    }
    #endregion
    // 清除显示数据
    void Clear()
    {
        FindCard = Brushes.Red;
        Uid10 = "";
        Uid10_ = "";
        Uid16 = "";
        Uid16_ = "";
        Ats = "";
        Status = State.等待放卡;
    }
    // 清除日志
    [RelayCommand]
    void ClearLogs()
    {
        LogDoc.Text = string.Empty;
    }
    // 添加日志
    public void AddLog(string msg)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            LogDoc.Insert(LogDoc.TextLength, msg + Environment.NewLine); 
            //滚动到最后一行
            
        });
    }
    // 显示消息框
    async Task ShowMessage(string title, string message)
    {
        await MessageBoxManager.GetMessageBoxStandard(title, message).ShowAsync();
    }
}
   