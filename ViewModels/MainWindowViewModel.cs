namespace D8_Demo.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public MenuViewModel MenuVm { get; }= new();
    public ContentViewModel ContentVm { get; }= new();
    public NavbarViewModel NavVm { get; }= new();
}