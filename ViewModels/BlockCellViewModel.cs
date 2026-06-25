using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace D8_Demo.ViewModels;

public partial class BlockCellViewModel : ViewModelBase
{
    private readonly M1ReadViewModel _owner;

    public int SectorIndex { get; }
    public int BlockIndex { get; }

    [ObservableProperty]
    private string data = "";

    public bool IsSelected =>
        _owner.SelectedSectorIndex == SectorIndex && _owner.SelectedBlockIndex == BlockIndex;

    public BlockCellViewModel(M1ReadViewModel owner, int sectorIndex, int blockIndex)
    {
        _owner = owner;
        SectorIndex = sectorIndex;
        BlockIndex = blockIndex;
        _owner.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(M1ReadViewModel.SelectedSectorIndex)
                or nameof(M1ReadViewModel.SelectedBlockIndex))
            {
                OnPropertyChanged(nameof(IsSelected));
            }
        };
    }

    [RelayCommand]
    private void Select() => _owner.SelectBlock(SectorIndex, BlockIndex);
}
