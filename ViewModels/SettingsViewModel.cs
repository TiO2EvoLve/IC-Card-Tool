using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using D8_Demo.Models;
using D8_Demo.Tool;
using D8_Demo.Views;

namespace D8_Demo.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    [ObservableProperty] private string selectedTheme;
    [ObservableProperty] private int sleepTime;
    [ObservableProperty] private bool sound;
    [ObservableProperty] private bool enableMica;

    private readonly AppSettings Settings = AppSettings.Instance;
    public string[] Themes { get; } = ["Light", "Dark", "Default"];

    public SettingsViewModel()
    {
        SelectedTheme = SettingsManage.Settings.Theme;
        SleepTime = SettingsManage.Settings.SleepTime;
        Sound = SettingsManage.Settings.Sound;
        EnableMica = SettingsManage.Settings.EnableMica;

        Settings.Theme = SelectedTheme;
        Settings.SleepTime = SleepTime;
        Settings.Sound = Sound;
        Settings.EnableMica = EnableMica;

        SetTheme(SelectedTheme);
    }

    public static SettingsViewModel Instance { get; } = new();

    [RelayCommand]
    private void Save(Window window)
    {
        SettingsManage.Settings.Theme = SelectedTheme;
        SettingsManage.Settings.SleepTime = SleepTime;
        SettingsManage.Settings.Sound = Sound;
        SettingsManage.Settings.EnableMica = EnableMica;
        SettingsManage.Save();

        SetTheme(SelectedTheme);
        Settings.Theme = SelectedTheme;
        Settings.SleepTime = SleepTime;
        Settings.Sound = Sound;
        Settings.EnableMica = EnableMica;
        ApplyWindowTransparency(window);
    }

    [RelayCommand]
    private void Reset(Window window)
    {
        Settings.Reset();
        SelectedTheme = Settings.Theme;
        SleepTime = Settings.SleepTime;
        Sound = Settings.Sound;
        EnableMica = Settings.EnableMica;
        Save(window);
    }

    private void ApplyWindowTransparency(Window? window)
    {
        var targetWindow = window ?? MainWindow.Current;
        if (targetWindow is null)
        {
            return;
        }

        targetWindow.TransparencyLevelHint = EnableMica
            ? [WindowTransparencyLevel.Mica]
            : [];

        targetWindow.Background = EnableMica
            ? new SolidColorBrush(Colors.Transparent)
            : new SolidColorBrush(Colors.White);

        targetWindow.InvalidateVisual();
    }

    private void SetTheme(string themeName)
    {
        if (Application.Current is { } app)
        {
            app.RequestedThemeVariant = themeName switch
            {
                "Light" => ThemeVariant.Light,
                "Dark" => ThemeVariant.Dark,
                "Default" => ThemeVariant.Default,
                _ => ThemeVariant.Default
            };
        }
    }
}