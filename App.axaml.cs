
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using D8_Demo.Service;
using D8_Demo.Tool;
using D8_Demo.ViewModels;
using D8_Demo.Views;
using D8_Demo.Views.UserControl;
using DryIoc;

namespace D8_Demo;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var container = new Container();
        
        container.Register<SettingsManage>(Reuse.Singleton);
        container.Register<CardHelper>(Reuse.Singleton);
        container.Register<Mdb>(Reuse.Singleton);
        container.Register<CardErrorService>(Reuse.Singleton);

        container.Register<MainWindowViewModel>(Reuse.Singleton);
        container.Register<MenuViewModel>(Reuse.Singleton);
        container.Register<ContentViewModel>(Reuse.Singleton);
        container.Register<SettingsViewModel>(Reuse.Singleton);
        container.Register<CardCheckViewModel>(Reuse.Singleton);
        container.Register<M1ReadViewModel>(Reuse.Singleton);
        container.Register<PSAMReaderViewModel>(Reuse.Singleton);

        container.Register<MainWindow>();
        container.Register<ReadView>();
        container.Register<CardCheckView>();
        container.Register<M1ReadView>();
        container.Register<PSAMReader>();
        container.Register<SettingView>();

        DataTemplates.Add(new ViewLocator(container));
        
        container.Resolve<CardErrorService>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = container.Resolve<MainWindow>();
            mainWindow.DataContext = container.Resolve<MainWindowViewModel>();
            mainWindow.ApplyMicaSetting(container.Resolve<SettingsManage>().Settings.EnableMica);
            desktop.MainWindow = mainWindow;
            desktop.Exit += (_, _) => container.Dispose();
        }

        base.OnFrameworkInitializationCompleted();
    }
}