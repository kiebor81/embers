using Embers.ISE.Services;
using Embers.ISE.ViewModels;
using Embers.ISE.Controls;
using Embers.ISE.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaEdit;
using AvaloniaEdit.CodeCompletion;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using AvaloniaEdit.Document;
using AvaloniaEdit.Folding;

namespace Embers.ISE.Views;

public partial class MainWindow : Window
{
    private TextEditor? _activeEditor;
    private CompletionWindow? _completionWindow;
    private readonly Dictionary<TextEditor, FoldingManager> _foldingManagers = new();
    private readonly HashSet<TextEditor> _configuredEditors = new();
    private readonly EmbersFoldingStrategy _foldingStrategy = new();
    private bool _pendingCtrlK;

    public MainWindow()
    {
        InitializeComponent();
        Opened += OnWindowOpened;
    }

    private void OnWindowOpened(object? sender, EventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            var console = this.FindControl<ColorizedConsole>("Console");
            var consoleScroll = this.FindControl<ScrollViewer>("ConsoleScroll");
            if (console != null)
            {
                viewModel.SetConsoleWriter(new ConsoleWriter(console, consoleScroll));
                Debug.WriteLine("Console writer configured");
            }
        }
    }

    private void InsertFunctionName(string name)
    {
        if (_activeEditor?.Document == null) return;

        var offset = _activeEditor.CaretOffset;
        _activeEditor.Document.Insert(offset, name);
        _activeEditor.CaretOffset = offset + name.Length;
    }

    private void OnFunctionItemPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Control control) return;
        if (control.DataContext is not FunctionListItem item) return;

        var properties = e.GetCurrentPoint(control).Properties;
        if (properties.IsRightButtonPressed)
        {
            OpenFunctionHelp(item.Name);
            e.Handled = true;
            return;
        }

        if (properties.IsLeftButtonPressed)
        {
            InsertFunctionName(item.Name);
            e.Handled = true;
        }
    }

    private void OnEditorTextEntered(TextEditor editor, TextInputEventArgs e)
    {
        if (editor.Document == null) return;

        if (e.Text == ".")
        {
            ShowCompletion(editor);
            return;
        }

        if (e.Text == ":")
        {
            var offset = editor.CaretOffset;
            if (offset >= 2 && editor.Document.GetText(offset - 2, 2) == "::")
            {
                ShowCompletion(editor);
            }
        }
    }

    private void OnEditorTextEntering(TextEditor editor, TextInputEventArgs e)
    {
        _ = editor;
        if (_completionWindow == null) return;
        if (string.IsNullOrEmpty(e.Text)) return;

        var ch = e.Text[0];
        if (!char.IsLetterOrDigit(ch) && ch != '_' && ch != '?')
        {
            _completionWindow.Close();
        }
    }

    private void ShowCompletion(TextEditor editor)
    {
        if (editor.Document == null) return;

        _completionWindow?.Close();
        _completionWindow = new CompletionWindow(editor.TextArea);
        _completionWindow.Closed += (_, _) => _completionWindow = null;

        var data = _completionWindow.CompletionList.CompletionData;
        var completions = CompletionService.GetCompletions(editor.Document.Text, editor.CaretOffset);
        if (completions.Count == 0)
            return;

        foreach (var item in completions)
        {
            data.Add(new SimpleCompletionData(item));
        }

        _completionWindow.Show();
    }

    private async void OpenFunctionHelp(string functionName)
    {
        var dialog = new FunctionHelpDialog();
        dialog.DataContext = new FunctionHelpViewModel(() => dialog.Close(), functionName);
        await dialog.ShowDialog(this);
    }

    private void OnEditorKeyDown(object? sender, KeyEventArgs e)
    {
        if (sender is not TextEditor editor || editor.Document == null) return;

        if (e.Key == Key.Escape)
        {
            _pendingCtrlK = false;
            return;
        }

        if (_pendingCtrlK)
        {
            if (e.Key == Key.C)
            {
                CommentSelection(editor);
                e.Handled = true;
            }
            else if (e.Key == Key.U)
            {
                UncommentSelection(editor);
                e.Handled = true;
            }

            _pendingCtrlK = false;
            return;
        }

        if (e.Key == Key.K && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            _pendingCtrlK = true;
            e.Handled = true;
        }
    }

    private void CommentSelection(TextEditor editor)
    {
        if (editor.Document == null) return;

        var document = editor.Document;
        var (startLine, endLine) = GetSelectedLineRange(editor);

        document.BeginUpdate();
        try
        {
            for (var lineNumber = endLine; lineNumber >= startLine; lineNumber--)
            {
                var line = document.GetLineByNumber(lineNumber);
                var lineText = document.GetText(line);
                var insertOffset = line.Offset + CountLeadingWhitespace(lineText);
                document.Insert(insertOffset, "#");
            }
        }
        finally
        {
            document.EndUpdate();
        }
    }

    private void UncommentSelection(TextEditor editor)
    {
        if (editor.Document == null) return;

        var document = editor.Document;
        var (startLine, endLine) = GetSelectedLineRange(editor);

        document.BeginUpdate();
        try
        {
            for (var lineNumber = endLine; lineNumber >= startLine; lineNumber--)
            {
                var line = document.GetLineByNumber(lineNumber);
                var lineText = document.GetText(line);
                var leading = CountLeadingWhitespace(lineText);
                if (leading >= lineText.Length) continue;

                if (lineText[leading] == '#')
                {
                    var removeOffset = line.Offset + leading;
                    var removeLength = 1;
                    if (leading + 1 < lineText.Length && lineText[leading + 1] == ' ')
                        removeLength = 2;

                    document.Remove(removeOffset, removeLength);
                }
            }
        }
        finally
        {
            document.EndUpdate();
        }
    }

    private (int StartLine, int EndLine) GetSelectedLineRange(TextEditor editor)
    {
        if (editor.Document == null) return (1, 1);

        var document = editor.Document;
        var selectionStart = editor.SelectionStart;
        var selectionLength = editor.SelectionLength;
        var selectionEnd = selectionStart + selectionLength;

        var startLine = document.GetLineByOffset(selectionStart).LineNumber;
        if (selectionLength == 0)
            return (startLine, startLine);

        var endLine = document.GetLineByOffset(selectionEnd).LineNumber;
        var endLineOffset = document.GetLineByNumber(endLine).Offset;
        if (selectionEnd == endLineOffset && selectionEnd > selectionStart)
            endLine = document.GetLineByOffset(selectionEnd - 1).LineNumber;

        return (startLine, endLine);
    }

    private static int CountLeadingWhitespace(string text)
    {
        var count = 0;
        while (count < text.Length && char.IsWhiteSpace(text[count]))
            count++;
        return count;
    }

    private void OnEditorCut(object? sender, RoutedEventArgs e) => _activeEditor?.Cut();

    private void OnEditorCopy(object? sender, RoutedEventArgs e) => _activeEditor?.Copy();

    private void OnEditorPaste(object? sender, RoutedEventArgs e) => _activeEditor?.Paste();

    private void OnEditorSelectAll(object? sender, RoutedEventArgs e)
    {
        if (_activeEditor?.Document == null) return;
        _activeEditor.Focus();
        _activeEditor.TextArea?.Focus();
        _activeEditor.SelectAll();
    }

    private void OnRunSelection(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel) return;
        var selection = _activeEditor?.SelectedText ?? string.Empty;
        viewModel.RunSelectionCommand.Execute(selection);
    }

    private void OnOpenGitHub(object? sender, RoutedEventArgs e)
    {
        const string url = "https://github.com/kiebor81/embers";
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to open GitHub URL: {ex.Message}");
        }
    }

    private void OnWindowKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel) return;

        if (e.Key == Key.F5)
        {
            viewModel.RunCommand.Execute(null);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.F6)
        {
            var selection = _activeEditor?.SelectedText ?? string.Empty;
            viewModel.RunSelectionCommand.Execute(selection);
            e.Handled = true;
        }
    }

    private void OnTabHeaderPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel) return;
        if (sender is not Control control) return;
        if (control.DataContext is not DocumentTab tab) return;

        var properties = e.GetCurrentPoint(control).Properties;
        if (properties.IsRightButtonPressed) return;

        viewModel.SelectedTab = tab;
        e.Handled = true;
    }

    private void OnScrollTabsLeft(object? sender, RoutedEventArgs e)
    {
        _ = sender;
        _ = e;
        var scroll = this.FindControl<ScrollViewer>("TabScroll");
        if (scroll == null) return;

        var next = Math.Max(0, scroll.Offset.X - 200);
        scroll.Offset = new Vector(next, scroll.Offset.Y);
    }

    private void OnScrollTabsRight(object? sender, RoutedEventArgs e)
    {
        _ = sender;
        _ = e;
        var scroll = this.FindControl<ScrollViewer>("TabScroll");
        if (scroll == null) return;

        scroll.Offset = new Vector(scroll.Offset.X + 200, scroll.Offset.Y);
    }

    private void OnEditorLoaded(object? sender, RoutedEventArgs e)
    {
        if (sender is not TextEditor editor) return;
        if (_configuredEditors.Contains(editor)) return;

        Debug.WriteLine("Applying editor configuration...");
        EditorConfiguration.ApplyEmbersHighlighting(editor);
        EditorConfiguration.ApplyDarkTheme(editor);
        Debug.WriteLine("Editor configuration applied");

        editor.Document ??= new TextDocument();
        var foldingManager = FoldingManager.Install(editor.TextArea);
        _foldingManagers[editor] = foldingManager;
        _foldingStrategy.UpdateFoldings(foldingManager, editor.Document);

        editor.TextArea.TextEntered += (_, args) => OnEditorTextEntered(editor, args);
        editor.TextArea.TextEntering += (_, args) => OnEditorTextEntering(editor, args);
        editor.TextArea.AddHandler(InputElement.KeyDownEvent, OnEditorKeyDown, RoutingStrategies.Tunnel);
        editor.TextChanged += OnEditorTextChanged;

        _configuredEditors.Add(editor);

        if (_activeEditor == null)
            _activeEditor = editor;
    }

    private void OnEditorUnloaded(object? sender, RoutedEventArgs e)
    {
        if (sender is not TextEditor editor) return;

        if (_foldingManagers.TryGetValue(editor, out var foldingManager))
        {
            FoldingManager.Uninstall(foldingManager);
            _foldingManagers.Remove(editor);
        }

        _configuredEditors.Remove(editor);
        if (ReferenceEquals(_activeEditor, editor))
            _activeEditor = null;
    }

    private void OnEditorTextChanged(object? sender, EventArgs e)
    {
        if (sender is not TextEditor editor || editor.Document == null) return;
        if (_foldingManagers.TryGetValue(editor, out var foldingManager))
            _foldingStrategy.UpdateFoldings(foldingManager, editor.Document);
    }

    private void OnEditorGotFocus(object? sender, GotFocusEventArgs e)
    {
        if (sender is TextEditor editor)
            _activeEditor = editor;
    }
}
