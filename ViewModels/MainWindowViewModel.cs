using System.Collections.Generic;
using System.Threading.Tasks;

namespace D8_Demo.ViewModels;

public partial class MainWindowViewModel(
    MenuViewModel menuVm,
    ContentViewModel contentVm,
    SettingsViewModel settingsVm,
    CardCheckViewModel cardCheckVm,
    M1ReadViewModel m1ReadVm,
    PSAMReaderViewModel psamVm)
    : ViewModelBase
{
    public MenuViewModel MenuVm { get; } = menuVm;
    public ContentViewModel ContentVm { get; } = contentVm;
    public SettingsViewModel SettingsVm { get; } = settingsVm;
    public CardCheckViewModel CardCheckVm { get; } = cardCheckVm;
    public M1ReadViewModel M1ReadVm { get; } = m1ReadVm;
    public PSAMReaderViewModel PSAMVm { get; } = psamVm;

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
                _ = UpdateRunningStateAsync();
            }
        }
    }

    private async Task UpdateRunningStateAsync()
    {
        // 停止所有循环任务（并发发出停止请求）
        var stopTasks = new List<Task>
        {
            ContentVm.StopAsync(),
            CardCheckVm.StopAsync(),
        };
        // 等待短超时内停止完成，不阻塞 UI 过久
        await Task.WhenAny(Task.WhenAll(stopTasks), Task.Delay(1500));
    }
}