using CommunityToolkit.Mvvm.Input;
using D8_Demo.Views;

namespace D8_Demo.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public MenuViewModel MenuVm { get; } = new();
    public ContentViewModel ContentVm { get; } = ContentViewModel.Instance;
    public SettingsViewModel SettingsVm { get; }= SettingsViewModel.Instance;
    
    public CardCheckViewModel CardCheckVm { get; } = new();
}