using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using D8_Demo.Views;

namespace D8_Demo.ViewModels;

public partial class SectorViewModel : ViewModelBase
{
    [ObservableProperty]
    private string? title;

    [ObservableProperty]
    private string? content;

    public SectorViewModel(int index)
    {
        Title = $"{index}扇区";
    }
    [RelayCommand]
    private void OnEdit()
    {
        var editor = new M1SectorEditor();
        editor.Show();
    }
}