using System.Collections.Generic;
using System.Threading.Tasks;

namespace D8_Demo.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public MenuViewModel MenuVm { get; } = new();
    public ContentViewModel ContentVm { get; } = ContentViewModel.Instance;
    public SettingsViewModel SettingsVm { get; }= SettingsViewModel.Instance;
    public CardCheckViewModel CardCheckVm { get; } = CardCheckViewModel.Instance;
    public M1ReadViewModel M1ReadVm { get; } = M1ReadViewModel.Instance;
    public PSAMReaderViewModel PSAMVm { get; } = PSAMReaderViewModel.Instance;


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