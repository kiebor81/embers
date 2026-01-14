using Avalonia.Media;
using Avalonia.Platform;
using AvaloniaEdit;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using System;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace Embers.ISE.Services;

public static class EditorConfiguration
{
    public static void ApplyEmbersHighlighting(TextEditor editor)
    {
        try
        {
            Debug.WriteLine("[EditorConfig] Loading syntax highlighting...");
            Stream? stream = null;
            var assetUri = new Uri("avares://Embers.ISE/Assets/Embers.xshd");

            if (AssetLoader.Exists(assetUri))
            {
                stream = AssetLoader.Open(assetUri);
                Debug.WriteLine("[EditorConfig] Loaded highlighting from Avalonia asset.");
            }
            else
            {
                var assembly = typeof(EditorConfiguration).Assembly;
                Debug.WriteLine($"[EditorConfig] Assembly: {assembly.FullName}");
                stream = assembly.GetManifestResourceStream("Embers.ISE.Assets.Embers.xshd");
                Debug.WriteLine(stream == null
                    ? "[EditorConfig] WARNING: Embedded resource not found"
                    : "[EditorConfig] Loaded highlighting from embedded resource.");
            }

            if (stream == null)
            {
                return;
            }

            using (stream)
            using (var reader = new XmlTextReader(stream))
            {
                var highlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                editor.SyntaxHighlighting = highlighting;
                Debug.WriteLine("[EditorConfig] Syntax highlighting applied");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[EditorConfig] ERROR loading syntax highlighting: {ex}");
        }
    }

    public static void ApplyDarkTheme(TextEditor editor)
    {
        Debug.WriteLine("[EditorConfig] Applying dark theme");
        
        // Ensure editor is editable
        editor.IsReadOnly = false;
        editor.Options.AllowScrollBelowDocument = true;
        
        Debug.WriteLine($"[EditorConfig] IsReadOnly: {editor.IsReadOnly}");
        Debug.WriteLine($"[EditorConfig] Document is null: {editor.Document == null}");
        
        // VS Code Dark+ theme colors
        editor.Background = new SolidColorBrush(Color.Parse("#1E1E1E"));
        editor.Foreground = new SolidColorBrush(Color.Parse("#D4D4D4"));
        editor.LineNumbersForeground = new SolidColorBrush(Color.Parse("#858585"));
        
        if (editor.TextArea != null)
        {
            editor.TextArea.TextView.LinkTextForegroundBrush = new SolidColorBrush(Color.Parse("#3794FF"));
            editor.TextArea.SelectionBrush = new SolidColorBrush(Color.Parse("#264F78"));
            editor.TextArea.SelectionForeground = new SolidColorBrush(Color.Parse("#FFFFFF"));
        }
        
        Debug.WriteLine("[EditorConfig] Dark theme applied");
    }
}
