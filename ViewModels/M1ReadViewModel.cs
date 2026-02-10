using System;
using CommunityToolkit.Mvvm.Input;
using D8_Demo.Tool;
using MsBox.Avalonia;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading;


namespace D8_Demo.ViewModels;

public partial class M1ReadViewModel : ViewModelBase
{
    [ObservableProperty] private byte index = 0;
    [ObservableProperty] private string passWorld = "FFFFFFFFFFFF";
    public static M1ReadViewModel Instance { get; } = new();
    private readonly ContentViewModel CVM = ContentViewModel.Instance;
    public ObservableCollection<SectorViewModel> Sectors { get; } = new();
    
    private readonly CardHelper CardHelper = CardHelper.Instance;

    private CancellationTokenSource? _cts;
    private Task? _runningTask;

    public M1ReadViewModel()
    {
        for (int i = 0; i < 16; i++)
        {
            Sectors.Add(new SectorViewModel(i));
        }
    }

    [RelayCommand]
    private void Read()
    {
        _ = StartAsync();
    }

    public Task StartAsync()
    {
        if (_cts != null) return Task.CompletedTask;
        _cts = new CancellationTokenSource();
        _runningTask = Task.Run(() => M1ReadSector(_cts.Token));
        return Task.CompletedTask;
    }

    public async Task StopAsync(TimeSpan? timeout = null)
    {
        if (_cts == null) return;
        _cts.Cancel();
        timeout ??= TimeSpan.FromSeconds(2);
        try { await Task.WhenAny(_runningTask ?? Task.CompletedTask, Task.Delay(timeout.Value)); } catch { }
        _cts.Dispose(); _cts = null; _runningTask = null;
    }

    private async Task M1ReadSector(CancellationToken token)
    {
        if (!CardHelper.isConnected)
        {
            await MessageBoxManager.GetMessageBoxStandard("警告", "请先打开端口").ShowAsync();
            return;
        }

        CardHelper.Reset();
        if (CardHelper.FindCard() ==0)
        {
            await MessageBoxManager.GetMessageBoxStandard("警告", "寻卡失败").ShowAsync();
            return;
        }
        string content = "";
        //验证卡密码
        for (byte i = 0; i < 16 && !token.IsCancellationRequested; i++)
        {
            for (byte j = 0; j < 4 && !token.IsCancellationRequested; j++)
            {
                if (CardHelper.AuthenticationPass(0x00, (byte)(4 * i + j), PassWorld))
                {
                    content =content + CardHelper.M1ReadSector((byte)(4 * i + j)).Substring(0,32) + "\n";
                }
                else
                {
                    content = "密码错误";
                }
            }
            Sectors[i].Content = content[..^1];
            content = "";
        }
    }
}