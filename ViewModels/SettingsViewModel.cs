
using System.Net.Mime;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using D8_Demo.Tool;


namespace D8_Demo.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{

    [ObservableProperty] private string selectedTheme;
    
    public string[] Themes { get; } = ["Light", "Dark","Default"];

    public SettingsViewModel()
    {
        // 加载现有配置
        SelectedTheme = SettingsManage.Settings.Theme;
        SetTheme(SelectedTheme);
    }

    public static SettingsViewModel Instance { get; } = new();

    [RelayCommand]
    private void Save(Window window)
    {
        SettingsManage.Settings.Theme = SelectedTheme;
        SettingsManage.Save();
        SetTheme(SelectedTheme);

    }

    [RelayCommand]
    private void Reset(Window window)
    {
        SelectedTheme = "Default";
        Save(window);
    }

    private void SetTheme(string themeName)
    {
        if (Application.Current is { } app)
        {
            app.RequestedThemeVariant = themeName switch
            {
                "Light" => ThemeVariant.Light,
                "Dark" => ThemeVariant.Dark,
                "Default" => ThemeVariant.Default // 跟随系统
            };
        }
    }
}