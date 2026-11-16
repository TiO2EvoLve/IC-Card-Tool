
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
    public static Container Container { get; private set; }
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Container = new Container();
        
        Container.Register<SettingsManage>(Reuse.Singleton);
        Container.Register<CardHelper>(Reuse.Singleton);
        Container.Register<Mdb>(Reuse.Singleton);
        Container.Register<CardErrorService>(Reuse.Singleton);

        Container.Register<MainWindowViewModel>(Reuse.Singleton);
        Container.Register<MenuViewModel>(Reuse.Singleton);
        Container.Register<ContentViewModel>(Reuse.Singleton);
        Container.Register<SettingsViewModel>(Reuse.Singleton);
        Container.Register<CardCheckViewModel>(Reuse.Singleton);
        Container.Register<M1ReadViewModel>(Reuse.Singleton);
        Container.Register<LoadM1KeyViewModel>(Reuse.Singleton);

        Container.Register<MainWindow>();
        Container.Register<ReadView>();
        Container.Register<CardCheckView>();
        Container.Register<M1ReadView>();
        Container.Register<SettingView>();
        Container.Register<LoadM1KeyView>();

        DataTemplates.Add(new ViewLocator(Container));
        
        Container.Resolve<CardErrorService>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = Container.Resolve<MainWindow>();
            mainWindow.DataContext = Container.Resolve<MainWindowViewModel>();
            mainWindow.ApplyMicaSetting(Container.Resolve<SettingsManage>().Settings.EnableMica);
            desktop.MainWindow = mainWindow;
            desktop.Exit += (_, _) => Container.Dispose();
        }

        base.OnFrameworkInitializationCompleted();
    }
}