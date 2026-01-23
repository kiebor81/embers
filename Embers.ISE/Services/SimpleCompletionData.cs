using Avalonia.Media;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Document;
using System;

namespace Embers.ISE.Services;

public sealed class SimpleCompletionData(string text, int? replaceStart = null, int? replaceEnd = null) : ICompletionData
{
    public IImage? Image => null;
    public string Text { get; } = text;
    public object Content { get; } = text;
    public object Description => Text;
    public double Priority => 0;

    public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
    {
        if (replaceStart.HasValue && replaceEnd.HasValue)
        {
            var start = Math.Max(0, Math.Min(replaceStart.Value, textArea.Document.TextLength));
            var end = Math.Max(0, Math.Min(replaceEnd.Value, textArea.Document.TextLength));
            if (end < start)
            {
                (start, end) = (end, start);
            }

            textArea.Document.Replace(start, end - start, Text);
            return;
        }

        textArea.Document.Replace(completionSegment, Text);
    }
}
