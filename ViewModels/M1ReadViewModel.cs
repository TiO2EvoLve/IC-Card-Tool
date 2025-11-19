using CommunityToolkit.Mvvm.Input;
using D8_Demo.Tool;
using MsBox.Avalonia;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls.Shapes;
using CommunityToolkit.Mvvm.ComponentModel;


namespace D8_Demo.ViewModels;

public partial class M1ReadViewModel : ViewModelBase
{
    [ObservableProperty] private byte index = 0;
    [ObservableProperty] private string passWorld = "FFFFFFFFFFFFFFFF";
    public static M1ReadViewModel Instance { get; } = new();
    private readonly ContentViewModel CVM = ContentViewModel.Instance;
    public ObservableCollection<SectorViewModel> Sectors { get; } = new();

    // 假设你有一个 CardHelper 实例，需先创建实例
    private readonly CardHelper CardHelper = new();

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
        Task.Run(M1ReadSector);
    }

    private async Task M1ReadSector()
    {
        if (!CVM.isConnected)
        {
            await MessageBoxManager.GetMessageBoxStandard("警告", "请先打开端口").ShowAsync();
            return;
        }

        CardHelper.Reset();
        if (!CardHelper.FindCard())
        {
            await MessageBoxManager.GetMessageBoxStandard("警告", "寻卡失败").ShowAsync();
            return;
        }
        string content = "";
        //验证卡密码
        byte[] password = new byte[7];
        password[0] = 0xff;
        password[1] = 0xff;
        password[2] = 0xff;
        password[3] = 0xff;
        password[4] = 0xff;
        password[5] = 0xff;
        password[6] = 0xff;
        for (byte i = 0; i < 16; i++)
        {
            for (byte j = 0; j < 4; j++)
            {
                if (CardHelper.AuthenticationPass(0x00, (byte)(4 * i + j), password))
                {
                    content =content + CardHelper.M1ReadSector((byte)(4 * i + j)).Substring(0,32) + "\n";
                }
                else
                {
                    content = "密码验证失败";
                }
            }
            Sectors[i].Content = content[..^1];
            content = "";
        }
    }
}