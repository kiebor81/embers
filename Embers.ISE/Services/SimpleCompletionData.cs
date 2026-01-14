using Avalonia.Media;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Document;
using System;

namespace Embers.ISE.Services;

public sealed class SimpleCompletionData(string text) : ICompletionData
{
    public IImage? Image => null;
    public string Text { get; } = text;
    public object Content { get; } = text;
    public object Description => Text;
    public double Priority => 0;

    public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs) => textArea.Document.Replace(completionSegment, Text);
}
