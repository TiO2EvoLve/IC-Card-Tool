using Avalonia;
using Avalonia.Controls;
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

    private readonly SettingsManage _settingsManage;
    private readonly AppSettings Settings;
    public string[] Themes { get; } = ["Light", "Dark", "Default"];

    public SettingsViewModel(SettingsManage settingsManage)
    {
        _settingsManage = settingsManage;
        Settings = settingsManage.Settings;
        SelectedTheme = Settings.Theme;
        SleepTime = Settings.SleepTime;
        Sound = Settings.Sound;
        EnableMica = Settings.EnableMica;

        Settings.Theme = SelectedTheme;
        Settings.SleepTime = SleepTime;
        Settings.Sound = Sound;
        Settings.EnableMica = EnableMica;

        SetTheme(SelectedTheme);
    }

    [RelayCommand]
    private void Save(Window window)
    {
        Settings.Theme = SelectedTheme;
        Settings.SleepTime = SleepTime;
        Settings.Sound = Sound;
        Settings.EnableMica = EnableMica;
        _settingsManage.Save();

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

    private void ApplyWindowTransparency(Window window)
    {
        if (window is MainWindow mainWindow)
            mainWindow.ApplyMicaSetting(EnableMica);
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