using Avalonia;
using Avalonia.Data;
using AvaloniaEdit;
using System.Diagnostics;

namespace Embers.ISE.Behaviors;

public static class TextEditorBindingBehavior
{
    public static readonly AttachedProperty<string?> TextProperty =
        AvaloniaProperty.RegisterAttached<TextEditor, string?>(
            "Text",
            typeof(TextEditorBindingBehavior),
            defaultBindingMode: BindingMode.TwoWay);

    private static bool _isUpdating;
    private static readonly AttachedProperty<bool> IsHookedProperty =
        AvaloniaProperty.RegisterAttached<TextEditor, bool>(
            "IsHooked",
            typeof(TextEditorBindingBehavior));

    static TextEditorBindingBehavior()
    {
        Debug.WriteLine("[TextEditorBinding] Static constructor called - registering property handler");
        TextProperty.Changed.AddClassHandler<TextEditor>(OnTextPropertyChanged);
    }

    public static string? GetText(TextEditor textEditor) => textEditor.GetValue(TextProperty);

    public static void SetText(TextEditor textEditor, string? value) => textEditor.SetValue(TextProperty, value);

    private static void OnTextPropertyChanged(TextEditor textEditor, AvaloniaPropertyChangedEventArgs e)
    {
        if (_isUpdating) return;

        Debug.WriteLine($"[TextEditorBinding] Property changed. OldValue: {e.OldValue?.GetType().Name}, NewValue length: {(e.NewValue as string)?.Length ?? 0}");

        EnsureTextChangedHook(textEditor);

        // Ensure Document is initialized
        if (textEditor.Document == null)
        {
            Debug.WriteLine("[TextEditorBinding] Creating new Document");
            textEditor.Document = new AvaloniaEdit.Document.TextDocument();
        }

        var newValue = e.NewValue as string;
        
        if (textEditor.Document.Text == newValue)
        {
            Debug.WriteLine("[TextEditorBinding] Text already matches, skipping update");
            return;
        }

        Debug.WriteLine($"[TextEditorBinding] Setting document text to {newValue?.Length ?? 0} chars");
        _isUpdating = true;
        textEditor.Document.Text = newValue ?? "";
        _isUpdating = false;
    }

    private static void EnsureTextChangedHook(TextEditor textEditor)
    {
        if (textEditor.GetValue(IsHookedProperty))
        {
            return;
        }

        Debug.WriteLine("[TextEditorBinding] Hooking up TextChanged event");
        textEditor.TextChanged += (sender, args) =>
        {
            if (_isUpdating)
            {
                Debug.WriteLine("[TextEditorBinding] TextChanged ignored (updating)");
                return;
            }

            Debug.WriteLine($"[TextEditorBinding] TextChanged fired, text length: {textEditor.Document?.Text?.Length ?? 0}");
            _isUpdating = true;
            SetText(textEditor, textEditor.Document?.Text ?? "");
            _isUpdating = false;
        };

        textEditor.SetValue(IsHookedProperty, true);
    }
}
