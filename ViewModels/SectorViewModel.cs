using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace D8_Demo.ViewModels;

public partial class SectorViewModel : ViewModelBase
{
    public int SectorIndex { get; }

    [ObservableProperty]
    private string title = "";

    public ObservableCollection<BlockCellViewModel> Blocks { get; }

    public SectorViewModel(int index, M1ReadViewModel owner)
    {
        SectorIndex = index;
        Title = $"{index}".PadLeft(2, '0') + "扇区";
        Blocks = new ObservableCollection<BlockCellViewModel>(
            Enumerable.Range(0, 4).Select(b => new BlockCellViewModel(owner, index, b)));
    }

    public void SetBlockData(int blockIndex, string data) => Blocks[blockIndex].Data = data;
}
