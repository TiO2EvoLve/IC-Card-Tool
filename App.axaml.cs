using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using D8_Demo.Service;
using D8_Demo.ViewModels;
using D8_Demo.Views;
using DryIoc;
using Microsoft.Extensions.Configuration;


namespace D8_Demo;

public partial class App : Application
{
    public static IConfigurationRoot Configuration { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        
        Configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory) // 设置基目录
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // 1. 创建容器
        var container = new Container();
        
        // 注册错误信息服务为单例
        container.Register<CardErrorService>(Reuse.Singleton);

        // 注册 ViewModel
        container.Register<MainWindowViewModel>();
        // 应用启动时提前加载 TOML
        container.Resolve<CardErrorService>();
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = container.Resolve<MainWindowViewModel>(),
            };
        }
        base.OnFrameworkInitializationCompleted();
    }
    
}