using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;

namespace Embers.ISE.Controls;

/// <summary>
/// A TextBlock-based console output control that supports inline colored text segments.
/// </summary>
public class ColorizedConsole : TextBlock
{
    public ColorizedConsole()
    {
        FontFamily = new FontFamily("Consolas, Courier New, monospace");
        FontSize = 13;
        Background = new SolidColorBrush(Color.Parse("#1E1E1E"));
        Foreground = new SolidColorBrush(Color.Parse("#CCCCCC"));
        TextWrapping = TextWrapping.Wrap;
        Padding = new Thickness(8);
    }

    public void Clear() => Inlines?.Clear();

    public void AppendText(string text, Color? color = null)
    {
        if (string.IsNullOrEmpty(text)) return;

        var run = new Run(text);
        if (color.HasValue)
        {
            run.Foreground = new SolidColorBrush(color.Value);
        }

        Inlines?.Add(run);
    }

    public void AppendLine(string text, Color? color = null) => AppendText(text + "\n", color);

    // Helper methods for common console colors
    public void AppendError(string text) => AppendText(text, Color.Parse("#F48771")); // Light red

    public void AppendSuccess(string text) => AppendText(text, Color.Parse("#4EC9B0")); // Cyan/teal

    public void AppendWarning(string text) => AppendText(text, Color.Parse("#CE9178")); // Orange

    public void AppendInfo(string text) => AppendText(text, Color.Parse("#569CD6")); // Blue

    public void AppendPrompt(string text) => AppendText(text, Color.Parse("#C586C0")); // Purple
}
