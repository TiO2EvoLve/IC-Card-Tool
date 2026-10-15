using System.Collections.Generic;
using System.Threading.Tasks;

namespace D8_Demo.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public MenuViewModel MenuVm { get; }
    public ContentViewModel ContentVm { get; }
    public SettingsViewModel SettingsVm { get; }
    public CardCheckViewModel CardCheckVm { get; }
    public M1ReadViewModel M1ReadVm { get; }
    public PSAMReaderViewModel PSAMVm { get; }
    public CheckErrorViewModel CheckErrorVm { get; }

    public MainWindowViewModel(
        MenuViewModel menuVm,
        ContentViewModel contentVm,
        SettingsViewModel settingsVm,
        CardCheckViewModel cardCheckVm,
        M1ReadViewModel m1ReadVm,
        PSAMReaderViewModel psamVm,
        CheckErrorViewModel checkErrorVm)
    {
        MenuVm = menuVm;
        ContentVm = contentVm;
        SettingsVm = settingsVm;
        CardCheckVm = cardCheckVm;
        M1ReadVm = m1ReadVm;
        PSAMVm = psamVm;
        CheckErrorVm = checkErrorVm;
    }

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