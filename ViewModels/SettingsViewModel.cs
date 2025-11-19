using System.Threading.Tasks;
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
    [ObservableProperty] private int sleepTime;
    [ObservableProperty] private bool sound;
    ContentViewModel contentViewModel => ContentViewModel.Instance;
    CardCheckViewModel cardCheckViewModel => CardCheckViewModel.Instance;
    public string[] Themes { get; } = ["Light", "Dark","Default"];

    public SettingsViewModel()
    {
        // 加载现有配置
        SelectedTheme = SettingsManage.Settings.Theme;//加载主题
        contentViewModel.ReadTime = SettingsManage.Settings.SleepTime;//设置读卡间隔
        cardCheckViewModel.ReadTime = SettingsManage.Settings.SleepTime;//设置读卡间隔
        SleepTime = contentViewModel.ReadTime;//设置读卡间隔
        Sound = SettingsManage.Settings.Sound;//设置是否开启声音
        contentViewModel.BeepSound = Sound;//设置是否开启声音
        SetTheme(SelectedTheme); //设置主题色
    }

    public static SettingsViewModel Instance { get; } = new();

    [RelayCommand]
    private async Task Save(Window window)
    {
        SettingsManage.Settings.Theme = SelectedTheme;//设置主题
        SettingsManage.Settings.SleepTime = SleepTime;//设置读卡间隔
        SettingsManage.Settings.Sound = Sound;
        contentViewModel.ReadTime = SleepTime;//设置读卡间隔
        cardCheckViewModel.ReadTime = SleepTime;//设置读卡间隔
        contentViewModel.BeepSound = Sound;//设置是否开启声音
        SettingsManage.Save();
        SetTheme(SelectedTheme);
        //await MessageBoxManager.GetMessageBoxStandard("成功", "保存成功").ShowAsync();
    }

    [RelayCommand]
    private void Reset(Window window)
    {
        SelectedTheme = "Default";
        SleepTime = 2000;
        Sound = false;
        _ = Save(window);
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