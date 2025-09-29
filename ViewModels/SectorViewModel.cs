using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace D8_Demo.ViewModels;

public partial class SectorViewModel : ViewModelBase
{
    [ObservableProperty]
    private string title;

    [ObservableProperty]
    private string content;

    public SectorViewModel(int index)
    {
        Title = $"{index}扇区";
    }

    [RelayCommand]
    private void Read()
    {
        // 这里的逻辑只作用于当前扇区
        Console.WriteLine($"读取 {Title}, 内容: {Content}");
    }

    [RelayCommand]
    private void Write()
    {
        Console.WriteLine($"写入 {Title}, 内容: {Content}");
    }
}