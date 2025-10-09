using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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
    
}