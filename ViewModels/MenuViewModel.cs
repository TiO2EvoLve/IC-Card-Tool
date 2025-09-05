using System;
using CommunityToolkit.Mvvm.Input;

namespace D8_Demo.ViewModels;

public partial class MenuViewModel : ViewModelBase
{
    [RelayCommand(CanExecute = nameof(CanNew))]
    private void New()
    {
        Console.WriteLine("New file created.");
    }

    [RelayCommand]
    private void Open()
    {
        Console.WriteLine("Open file dialog.");
    }

    [RelayCommand]
    private void Exit()
    {
        Environment.Exit(0);
    }
    
    private bool CanNew()
    {
        return true;
    }
}