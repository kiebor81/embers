using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Embers.ISE.Controls;
using System;

namespace Embers.ISE.ViewModels;

public class ConsoleWriter(ColorizedConsole? console, ScrollViewer? scrollViewer = null)
{
    private readonly ColorizedConsole? _console = console;
    private readonly ScrollViewer? _scrollViewer = scrollViewer;

    public void Clear()
    {
        if (_console == null) return;
        Dispatcher.UIThread.Post(() => _console.Clear());
    }

    public void Write(string text, ConsoleColor color = ConsoleColor.Gray)
    {
        if (_console == null || string.IsNullOrEmpty(text)) return;
        
        Dispatcher.UIThread.Post(() =>
        {
            var avalColor = GetColor(color);
            _console.AppendText(text, avalColor);
            ScrollToEnd();
        });
    }

    public void WriteLine(string text = "", ConsoleColor color = ConsoleColor.Gray) => Write(text + "\n", color);

    public void WriteError(string text)
    {
        if (_console == null) return;
        Dispatcher.UIThread.Post(() =>
        {
            _console.AppendError(text);
            ScrollToEnd();
        });
    }

    public void WriteSuccess(string text)
    {
        if (_console == null) return;
        Dispatcher.UIThread.Post(() =>
        {
            _console.AppendSuccess(text);
            ScrollToEnd();
        });
    }

    public void WriteWarning(string text)
    {
        if (_console == null) return;
        Dispatcher.UIThread.Post(() =>
        {
            _console.AppendWarning(text);
            ScrollToEnd();
        });
    }

    public void WriteInfo(string text)
    {
        if (_console == null) return;
        Dispatcher.UIThread.Post(() =>
        {
            _console.AppendInfo(text);
            ScrollToEnd();
        });
    }

    public void WritePrompt(string text)
    {
        if (_console == null) return;
        Dispatcher.UIThread.Post(() =>
        {
            _console.AppendPrompt(text);
            ScrollToEnd();
        });
    }

    private void ScrollToEnd()
    {
        if (_scrollViewer == null) return;
        _scrollViewer.Offset = new Vector(_scrollViewer.Offset.X, _scrollViewer.Extent.Height);
    }

    private static Color GetColor(ConsoleColor color) => color switch
    {
        ConsoleColor.Black => Color.Parse("#000000"),
        ConsoleColor.DarkBlue => Color.Parse("#000080"),
        ConsoleColor.DarkGreen => Color.Parse("#008000"),
        ConsoleColor.DarkCyan => Color.Parse("#008080"),
        ConsoleColor.DarkRed => Color.Parse("#800000"),
        ConsoleColor.DarkMagenta => Color.Parse("#800080"),
        ConsoleColor.DarkYellow => Color.Parse("#808000"),
        ConsoleColor.Gray => Color.Parse("#C0C0C0"),
        ConsoleColor.DarkGray => Color.Parse("#808080"),
        ConsoleColor.Blue => Color.Parse("#569CD6"),
        ConsoleColor.Green => Color.Parse("#4EC9B0"),
        ConsoleColor.Cyan => Color.Parse("#4FC1FF"),
        ConsoleColor.Red => Color.Parse("#F48771"),
        ConsoleColor.Magenta => Color.Parse("#C586C0"),
        ConsoleColor.Yellow => Color.Parse("#DCDCAA"),
        ConsoleColor.White => Color.Parse("#D4D4D4"),
        _ => Color.Parse("#CCCCCC")
    };
}
