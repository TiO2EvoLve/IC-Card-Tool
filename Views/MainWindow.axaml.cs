using System;
using Avalonia.Controls;
using Avalonia.Threading;
using AvaloniaEdit.Document;

namespace D8_Demo.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
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