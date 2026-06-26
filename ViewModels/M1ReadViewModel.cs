using CommunityToolkit.Mvvm.Input;
using D8_Demo.Tool;
using MsBox.Avalonia;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace D8_Demo.ViewModels;

public partial class M1ReadViewModel : ViewModelBase
{
    [ObservableProperty] private string passWorld = "FFFFFFFFFFFF";
    public static M1ReadViewModel Instance { get; } = new();
    public ObservableCollection<SectorViewModel> Sectors { get; } = new();
    private readonly CardHelper CardHelper = CardHelper.Instance;
    private readonly ContentViewModel CVM = ContentViewModel.Instance;
    //日志内容
    
    [ObservableProperty] private string selectedBlockData = "";
    [ObservableProperty] private string selectedPosition = "未选择";
    [ObservableProperty] private int selectedSectorIndex = -1;
    [ObservableProperty] private int selectedBlockIndex = -1;

    public M1ReadViewModel()
    {
        for (int i = 0; i < 16; i++)
            Sectors.Add(new SectorViewModel(i, this));
    }

    public void SelectBlock(int sectorIndex, int blockIndex)
    {
        SelectedSectorIndex = sectorIndex;
        SelectedBlockIndex = blockIndex;
        RefreshSelectedDisplay();
    }

    private void RefreshSelectedDisplay()
    {
        if (SelectedSectorIndex < 0 || SelectedBlockIndex < 0)
        {
            SelectedBlockData = "";
            SelectedPosition = "未选择";
            return;
        }
        SelectedBlockData = Sectors[SelectedSectorIndex].Blocks[SelectedBlockIndex].Data;
        SelectedPosition = $"扇区{SelectedSectorIndex}，块{SelectedBlockIndex}";
    }

    [RelayCommand]
    private void Read() => _ = M1ReadSector();

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

        for (byte i = 0; i < 16; i++)
        {
            var sector = Sectors[i];
            for (byte j = 0; j < 4; j++)
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

                sector.SetBlockData(j, blockContent);
            }
        }

        RefreshSelectedDisplay();
    }

    [RelayCommand]
    public void Write() => _ = M1WriteSector();
    

    private async Task M1WriteSector()
    {
        if (SelectedBlockData.Length != 32)
        {
            await MessageBoxManager.GetMessageBoxStandard("警告", $"写入位数错误:{SelectedBlockData.Length}").ShowAsync();
            return;
        }
        
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

        if (!CardHelper.AuthenticationPass(0x00, (byte)(SelectedSectorIndex * 4 + SelectedBlockIndex), PassWorld))
        {
            CVM.AddLog("密码验证错误");
            return;
        }
        if(CardHelper.M1WriteSector(SelectedSectorIndex * 4 + SelectedBlockIndex, SelectedBlockData))
        {
            CVM.AddLog("M1修改成功");
            Read();
        }
        else
        {
            CVM.AddLog("M1修改失败");
        }

    }
}
