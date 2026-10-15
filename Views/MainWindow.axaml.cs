using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace D8_Demo.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
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