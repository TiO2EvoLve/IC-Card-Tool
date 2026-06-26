using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using D8_Demo.Models;
using D8_Demo.Tool;

namespace D8_Demo.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{

    [ObservableProperty] private string selectedTheme;
    [ObservableProperty] private int sleepTime;
    [ObservableProperty] private bool sound;

    private readonly AppSettings  Settings = AppSettings.Instance;
    public string[] Themes { get; } = ["Light", "Dark","Default"];

    public SettingsViewModel()
    {
        // 加载现有配置到UI
        SelectedTheme = SettingsManage.Settings.Theme;//加载主题
        SleepTime = SettingsManage.Settings.SleepTime;//设置读卡间隔
        Sound = SettingsManage.Settings.Sound;//设置是否开启声音
        
        Settings.Theme = SelectedTheme;
        Settings.SleepTime = SleepTime;
        Settings.Sound = Sound;
        
        SetTheme(SelectedTheme); //设置主题色
    }

    public static SettingsViewModel Instance { get; } = new();

    [RelayCommand]
    private void Save(Window window)
    {
        SettingsManage.Settings.Theme = SelectedTheme;//设置主题
        SettingsManage.Settings.SleepTime = SleepTime;//设置读卡间隔
        SettingsManage.Settings.Sound = Sound;
        SettingsManage.Save();
        
        SetTheme(SelectedTheme);
        Settings.Theme = SelectedTheme;
        Settings.SleepTime = SleepTime;
        Settings.Sound = Sound;
    }

    [RelayCommand]
    private void Reset(Window window)
    {
        Settings.Reset();
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