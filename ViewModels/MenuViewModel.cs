using System;
using CommunityToolkit.Mvvm.Input;

namespace D8_Demo.ViewModels;

public partial class MenuViewModel : ViewModelBase
{
    private readonly ContentViewModel ContentVM = ContentViewModel.Instance;
    
    [RelayCommand]
    private void Open()
    {
        _ = ContentVM.OpenPort();
    }

    [RelayCommand]
    private void Close()
    {
        _ = ContentVM.ClosePort();
    }

    [RelayCommand]
    private void Exit()
    {
        Environment.Exit(0);
    }
}