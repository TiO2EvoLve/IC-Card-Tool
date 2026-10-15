using System;
using CommunityToolkit.Mvvm.Input;

namespace D8_Demo.ViewModels;

public partial class MenuViewModel : ViewModelBase
{
    private readonly ContentViewModel _contentViewModel;

    public MenuViewModel(ContentViewModel contentViewModel)
    {
        _contentViewModel = contentViewModel;
    }

    [RelayCommand]
    private void Open()
    {
        _ = _contentViewModel.OpenPort();
    }

    [RelayCommand]
    private void Close()
    {
        _ = _contentViewModel.ClosePort();
    }

    [RelayCommand]
    private void Exit()
    {
        Environment.Exit(0);
    }
}