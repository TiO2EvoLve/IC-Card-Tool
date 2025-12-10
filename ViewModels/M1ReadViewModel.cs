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
    [ObservableProperty] private string passWorld = "FFFFFFFFFFFF";
    public static M1ReadViewModel Instance { get; } = new();
    private readonly ContentViewModel CVM = ContentViewModel.Instance;
    public ObservableCollection<SectorViewModel> Sectors { get; } = new();
    
    private readonly CardHelper CardHelper = CardHelper.Instance;

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
        
        for (byte i = 0; i < 16; i++)
        {
            for (byte j = 0; j < 4; j++)
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