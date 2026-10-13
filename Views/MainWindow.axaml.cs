using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using D8_Demo.Tool;

namespace D8_Demo.Views;

public partial class MainWindow : Window
{
    public static MainWindow? Current { get; private set; }

    public MainWindow()
    {
        InitializeComponent();
        Current = this;
        ApplyMicaSetting();
    }

    public void ApplyMicaSetting(bool? enableMica = null)
    {
        var shouldEnableMica = enableMica ?? SettingsManage.Settings.EnableMica;

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