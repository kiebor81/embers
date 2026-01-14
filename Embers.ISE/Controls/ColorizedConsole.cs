using System;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Media;

namespace Embers.ISE.Controls;

/// <summary>
/// A TextBlock-based console output control that supports inline colored text segments.
/// </summary>
public class ColorizedConsole : TextBlock
{
    private readonly StringBuilder _plainText = new();
    private Point _lastPointerPosition;

    public ColorizedConsole()
    {
        FontFamily = new FontFamily("Consolas, Courier New, monospace");
        FontSize = 13;
        Background = new SolidColorBrush(Color.Parse("#1E1E1E"));
        Foreground = new SolidColorBrush(Color.Parse("#CCCCCC"));
        TextWrapping = TextWrapping.Wrap;
        Padding = new Thickness(8);

        ContextMenu = BuildContextMenu();
        PointerPressed += OnPointerPressed;
    }

    public void Clear()
    {
        Inlines?.Clear();
        _plainText.Clear();
    }

    public void AppendText(string text, Color? color = null)
    {
        if (string.IsNullOrEmpty(text)) return;

        var run = new Run(text);
        if (color.HasValue)
        {
            run.Foreground = new SolidColorBrush(color.Value);
        }

        Inlines?.Add(run);
        _plainText.Append(text);
    }

    public void AppendLine(string text, Color? color = null) => AppendText(text + "\n", color);

    // Helper methods for common console colors
    public void AppendError(string text) => AppendText(text, Color.Parse("#F48771")); // Light red

    public void AppendSuccess(string text) => AppendText(text, Color.Parse("#4EC9B0")); // Cyan/teal

    public void AppendWarning(string text) => AppendText(text, Color.Parse("#CE9178")); // Orange

    public void AppendInfo(string text) => AppendText(text, Color.Parse("#569CD6")); // Blue

    public void AppendPrompt(string text) => AppendText(text, Color.Parse("#C586C0")); // Purple

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _lastPointerPosition = e.GetPosition(this);
    }

    private ContextMenu BuildContextMenu()
    {
        var copyLine = new MenuItem { Header = "Copy Line" };
        copyLine.Click += (_, _) => CopyLineAtLastPointerAsync();

        var copyAll = new MenuItem { Header = "Copy All" };
        copyAll.Click += (_, _) => CopyAllAsync();

        return new ContextMenu
        {
            Items =
            {
                copyLine,
                copyAll
            }
        };
    }

    private async void CopyLineAtLastPointerAsync()
    {
        var line = GetLineAtPoint(_lastPointerPosition);
        if (string.IsNullOrEmpty(line)) return;

        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard != null)
            await clipboard.SetTextAsync(line);
    }

    private async void CopyAllAsync()
    {
        var text = _plainText.ToString();
        if (string.IsNullOrEmpty(text)) return;

        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard != null)
            await clipboard.SetTextAsync(text);
    }

    private string? GetLineAtPoint(Point point)
    {
        if (_plainText.Length == 0)
            return null;

        var padding = Padding;
        var localY = Math.Max(0, point.Y - padding.Top);
        var lineHeight = LineHeight > 0 ? LineHeight : FontSize * 1.35;
        var lineIndex = (int)(localY / lineHeight);

        var lines = _plainText.ToString().Split('\n');
        if (lines.Length == 0)
            return null;

        lineIndex = Math.Clamp(lineIndex, 0, lines.Length - 1);
        return lines[lineIndex].TrimEnd('\r');
    }
}

