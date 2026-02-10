using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using D8_Demo.Views;

namespace D8_Demo.ViewModels;

public partial class SectorViewModel : ViewModelBase
{
    [ObservableProperty]
    private string? title;

    /// <summary>第1块(子扇区)内容</summary>
    [ObservableProperty]
    private string block0 = "";

    /// <summary>第2块(子扇区)内容</summary>
    [ObservableProperty]
    private string block1 = "";

    /// <summary>第3块(子扇区)内容</summary>
    [ObservableProperty]
    private string block2 = "";

    /// <summary>第4块(子扇区)内容</summary>
    [ObservableProperty]
    private string block3 = "";

    public SectorViewModel(int index)
    {
        Title = $"{index}".PadLeft(2, '0') + "扇区";
    }
}