using System;
using CommunityToolkit.Mvvm.Input;
using D8_Demo.Tool;
using MsBox.Avalonia;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Media;


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
        _ = M1ReadSector();
    }
    

    private async Task M1ReadSector()
    {
        if (!CardHelper.isConnected)
        {
            await MessageBoxManager.GetMessageBoxStandard("警告", "请先打开端口").ShowAsync();
            return;
        }

        CardHelper.Reset();
        if (CardHelper.FindCard() == 0)
        {
            await MessageBoxManager.GetMessageBoxStandard("警告", "寻卡失败").ShowAsync();
            return;
        }
        // 每个扇区4个块，分别写入 Block0~Block3
        for (byte i = 0; i < 16 ; i++)
        {
            var sector = Sectors[i];
            for (byte j = 0; j < 4 ; j++)
            {
                string blockContent;
                if (CardHelper.AuthenticationPass(0x00, (byte)(4 * i + j), PassWorld))
                {
                    var raw = CardHelper.M1ReadSector((byte)(4 * i + j));
                    blockContent = raw.Length >= 32 ? raw.Substring(0, 32) : raw;
                }
                else
                {
                    blockContent = "密码错误";
                }
                switch (j)
                {
                    case 0: sector.Block0 = blockContent; break;
                    case 1: sector.Block1 = blockContent; break;
                    case 2: sector.Block2 = blockContent; break;
                    case 3: sector.Block3 = blockContent; break;
                }
            }
        }
    }
    
    private void FitText(TextBox tb)
    {
        if (string.IsNullOrEmpty(tb.Text))
            return;

        double targetWidth = tb.Bounds.Width - 10;

        double fontSize = 100;

        while (fontSize > 1)
        {
            var text = new FormattedText(
                tb.Text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(tb.FontFamily),
                fontSize,
                Brushes.Black);

            if (text.Width <= targetWidth)
                break;

            fontSize--;
        }

        tb.FontSize = fontSize;
    }
}