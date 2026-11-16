using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using D8_Demo.ViewModels;

namespace D8_Demo.Views;

public partial class LoadM1KeyView : Window
{
    public LoadM1KeyView(LoadM1KeyViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}