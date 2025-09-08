using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using D8_Demo.Views;

namespace D8_Demo.ViewModels;

public partial class NavbarViewModel : ViewModelBase
{
    [ObservableProperty]
    private object? currentPage;
    public IRelayCommand<string> NavigateCommand { get; }

    public NavbarViewModel()
    {
        NavigateCommand = new RelayCommand<string>(Navigate);
        CurrentPage = new HomeView();
    }
    private void Navigate(string page)
    {
        switch (page)
        {
            case "Home":
                CurrentPage = new HomeView();
                break;
            case "Settings":
                CurrentPage = new CardReadView();
                break;
            case "About":
                CurrentPage = new SettingView();
                break;
        }
    }
}