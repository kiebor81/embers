using Embers.ISE.Services;
using Embers.ISE.ViewModels;
using Embers.ISE.Controls;
using Embers.ISE.Models;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaEdit;
using AvaloniaEdit.CodeCompletion;
using System;
using System.Diagnostics;
using AvaloniaEdit.Document;
using AvaloniaEdit.Folding;

namespace Embers.ISE.Views;

public partial class MainWindow : Window
{
    private bool _isUpdating;
    private TextEditor? _editor;
    private CompletionWindow? _completionWindow;
    private FoldingManager? _foldingManager;
    private readonly EmbersFoldingStrategy _foldingStrategy = new();

    public MainWindow()
    {
        InitializeComponent();
        Opened += OnWindowOpened;
    }

    private void OnWindowOpened(object? sender, EventArgs e)
    {
        var editor = this.FindControl<TextEditor>("Editor");
        _editor = editor;
        
        if (editor != null)
        {
            Debug.WriteLine("Applying editor configuration...");
            EditorConfiguration.ApplyEmbersHighlighting(editor);
            EditorConfiguration.ApplyDarkTheme(editor);
            Debug.WriteLine("Editor configuration applied");

            if (DataContext is MainWindowViewModel vm)
            {
                editor.Document ??= new TextDocument();
                editor.Document.Text = vm.EditorText ?? string.Empty;
                _foldingManager = FoldingManager.Install(editor.TextArea);
                _foldingStrategy.UpdateFoldings(_foldingManager, editor.Document);

                editor.TextArea.TextEntered += OnEditorTextEntered;
                editor.TextArea.TextEntering += OnEditorTextEntering;

                editor.TextChanged += (s, args) =>
                {
                    if (_isUpdating) return;
                    _isUpdating = true;
                    vm.EditorText = editor.Document?.Text ?? string.Empty;
                    if (_foldingManager != null && editor.Document != null)
                        _foldingStrategy.UpdateFoldings(_foldingManager, editor.Document);
                    _isUpdating = false;
                };

                vm.PropertyChanged += (s, args) =>
                {
                    if (args.PropertyName != nameof(vm.EditorText)) return;
                    if (_isUpdating) return;

                    var newText = vm.EditorText ?? string.Empty;
                    if (editor.Document?.Text == newText) return;

                    _isUpdating = true;
                    editor.Document ??= new TextDocument();
                    editor.Document.Text = newText;
                    if (_foldingManager != null)
                        _foldingStrategy.UpdateFoldings(_foldingManager, editor.Document);
                    _isUpdating = false;
                };
            }
        }

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
        if (_editor?.Document == null) return;

        var offset = _editor.CaretOffset;
        _editor.Document.Insert(offset, name);
        _editor.CaretOffset = offset + name.Length;
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

    private void OnEditorTextEntered(object? sender, TextInputEventArgs e)
    {
        if (_editor?.Document == null) return;

        if (e.Text == ".")
        {
            ShowCompletion();
            return;
        }

        if (e.Text == ":")
        {
            var offset = _editor.CaretOffset;
            if (offset >= 2 && _editor.Document.GetText(offset - 2, 2) == "::")
            {
                ShowCompletion();
            }
        }
    }

    private void OnEditorTextEntering(object? sender, TextInputEventArgs e)
    {
        if (_completionWindow == null) return;
        if (string.IsNullOrEmpty(e.Text)) return;

        var ch = e.Text[0];
        if (!char.IsLetterOrDigit(ch) && ch != '_' && ch != '?')
        {
            _completionWindow.Close();
        }
    }

    private void ShowCompletion()
    {
        if (_editor?.Document == null) return;

        _completionWindow?.Close();
        _completionWindow = new CompletionWindow(_editor.TextArea);
        _completionWindow.Closed += (_, _) => _completionWindow = null;

        var data = _completionWindow.CompletionList.CompletionData;
        var completions = CompletionService.GetCompletions(_editor.Document.Text, _editor.CaretOffset);
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

    private void OnEditorCut(object? sender, RoutedEventArgs e) => _editor?.Cut();

    private void OnEditorCopy(object? sender, RoutedEventArgs e) => _editor?.Copy();

    private void OnEditorPaste(object? sender, RoutedEventArgs e) => _editor?.Paste();

    private void OnEditorSelectAll(object? sender, RoutedEventArgs e)
    {
        if (_editor?.Document == null) return;
        _editor.Focus();
        _editor.TextArea?.Focus();
        _editor.SelectAll();
    }

    private void OnRunSelection(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel) return;
        var selection = _editor?.SelectedText ?? string.Empty;
        viewModel.RunSelectionCommand.Execute(selection);
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
            var selection = _editor?.SelectedText ?? string.Empty;
            viewModel.RunSelectionCommand.Execute(selection);
            e.Handled = true;
        }
    }
}
