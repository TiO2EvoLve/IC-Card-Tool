using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;

namespace D8_Demo.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        //默认全屏运行
        WindowState = WindowState.Maximized;
    }

    private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }
    private void Close(object? sender, PointerPressedEventArgs e)
    {
        Close();
    }

    private void MaxWindow(object? sender, PointerPressedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void MinWindow(object? sender, PointerPressedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }
    
    public void ApplyMicaSetting(bool shouldEnableMica)
    {
        TransparencyLevelHint = shouldEnableMica
            ? [WindowTransparencyLevel.Mica]
            : [];

        Background = shouldEnableMica
            ? new SolidColorBrush(Colors.Transparent)
            : new SolidColorBrush(Colors.White);

        InvalidateVisual();
    }

    private void LogEditor_OnTextChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            logEditor.CaretOffset = logEditor.Document.TextLength;
            logEditor.TextArea.Caret.BringCaretToView();
        });
    }
}