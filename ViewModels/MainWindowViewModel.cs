using System;
using CommunityToolkit.Mvvm.Input;
using D8_Demo.Views;

namespace D8_Demo.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public MenuViewModel MenuVm { get; } = new();
    public ContentViewModel ContentVm { get; } = ContentViewModel.Instance;
    public SettingsViewModel SettingsVm { get; }= SettingsViewModel.Instance;
    public CardCheckViewModel CardCheckVm { get; } = CardCheckViewModel.Instance;
    public M1ReadViewModel M1ReadVm { get; } = M1ReadViewModel.Instance;


    private int _selectedTabIndex;
    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set
        {
            if (_selectedTabIndex != value)
            {
                _selectedTabIndex = value;
                OnPropertyChanged(); 
                UpdateRunningState();
            }
        }
    }

    private void UpdateRunningState()
    {
        // 停止所有循环任务
        ContentVm.CanRun = false;
        CardCheckVm.CanRun = false;
        // 根据当前选择的页面允许对应 VM 的循环任务
        switch (SelectedTabIndex)
        {
            case 0: ContentVm.CanRun = true; break;
            case 1: CardCheckVm.CanRun = true; break;
        }
    }
}