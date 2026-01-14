using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Embers.Annotations;
using Embers.ISE.Services;
using Embers.ISE.Models;
using Embers.ISE.Views;
using Embers.Security;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Embers.ISE.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private readonly EmbersHost _host = new();
    private CancellationTokenSource? _cts;
    private string? _currentFile;
    private ConsoleWriter? _console;

    private string _editorText = "";
    public string EditorText
    {
        get => _editorText;
        set => Set(ref _editorText, value);
    }

    public ObservableCollection<FunctionListItem> StdLibFunctions { get; } = [];
    public ObservableCollection<FunctionListItem> HostFunctions { get; } = [];
    public ObservableCollection<string> ReferenceAssemblies { get; } = [];

    public ObservableCollection<string> WhitelistEntries { get; } = [];

    private readonly List<FunctionListItem> _allStdLibFunctions = [];
    private readonly List<FunctionListItem> _allHostFunctions = [];

    private string _stdLibFilterText = "";
    public string StdLibFilterText
    {
        get => _stdLibFilterText;
        set
        {
            if (Equals(_stdLibFilterText, value)) return;
            Set(ref _stdLibFilterText, value);
            ApplyFunctionFilters();
        }
    }

    private string _hostFilterText = "";
    public string HostFilterText
    {
        get => _hostFilterText;
        set
        {
            if (Equals(_hostFilterText, value)) return;
            Set(ref _hostFilterText, value);
            ApplyFunctionFilters();
        }
    }

    private string _newWhitelistEntry = "";
    public string NewWhitelistEntry
    {
        get => _newWhitelistEntry;
        set => Set(ref _newWhitelistEntry, value);
    }

    private string? _selectedWhitelistEntry;
    public string? SelectedWhitelistEntry
    {
        get => _selectedWhitelistEntry;
        set => Set(ref _selectedWhitelistEntry, value);
    }

    public SecurityMode[] SecurityModes { get; } = Enum.GetValues<SecurityMode>();

    private SecurityMode _selectedSecurityMode = SecurityMode.Unrestricted;
    public SecurityMode SelectedSecurityMode
    {
        get => _selectedSecurityMode;
        set
        {
            if (Equals(_selectedSecurityMode, value)) return;
            Set(ref _selectedSecurityMode, value);
            IsWhitelistMode = value == SecurityMode.WhitelistOnly;
            ApplySecurityPolicy();
        }
    }

    private bool _isWhitelistMode;
    public bool IsWhitelistMode
    {
        get => _isWhitelistMode;
        set => Set(ref _isWhitelistMode, value);
    }

    private string _consoleText = "";
    public string ConsoleText
    {
        get => _consoleText;
        set => Set(ref _consoleText, value);
    }

    private bool _isRightPanelVisible = true;
    public bool IsRightPanelVisible
    {
        get => _isRightPanelVisible;
        set
        {
            if (Equals(_isRightPanelVisible, value)) return;
            Set(ref _isRightPanelVisible, value);
            RightPanelWidth = value ? new GridLength(300) : new GridLength(0);
        }
    }

    private GridLength _rightPanelWidth = new(300);
    public GridLength RightPanelWidth
    {
        get => _rightPanelWidth;
        set => Set(ref _rightPanelWidth, value);
    }

    public ICommand OpenCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand NewCommand { get; }
    public ICommand AddReferenceCommand { get; }
    public ICommand ToggleRightPanelCommand { get; }
    public ICommand RemoveReferenceCommand { get; }
    public ICommand HelpCommand { get; }
    public ICommand RunCommand { get; }
    public ICommand RunSelectionCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand AddWhitelistEntryCommand { get; }
    public ICommand RemoveWhitelistEntryCommand { get; }
    public ICommand ClearWhitelistCommand { get; }

    public MainWindowViewModel()
    {
        // Route host output into ConsoleWriter
        _host.Stdout += s => AppendConsole(s, ConsoleColor.Gray);
        _host.Stderr += s => AppendConsole(s, ConsoleColor.Red);

        OpenCommand = new AsyncCommand(OpenAsync);
        SaveCommand = new AsyncCommand(SaveAsync);
        NewCommand = new Commands(NewScript);
        AddReferenceCommand = new AsyncCommand(AddReferenceAsync);
        ToggleRightPanelCommand = new Commands(ToggleRightPanel);
        RemoveReferenceCommand = new ParameterCommand(RemoveReference);
        HelpCommand = new AsyncCommand(OpenHelpAsync);
        RunCommand = new AsyncCommand(RunAsync);
        RunSelectionCommand = new AsyncParameterCommand(RunSelectionAsync);
        StopCommand = new Commands(Stop);
        AddWhitelistEntryCommand = new Commands(AddWhitelistEntry);
        RemoveWhitelistEntryCommand = new Commands(RemoveWhitelistEntry);
        ClearWhitelistCommand = new Commands(ClearWhitelist);

        RefreshFunctionLists();
        ApplySecurityPolicy();
    }

    private void NewScript()
    {
        _currentFile = null;
        EditorText = string.Empty;
        _console?.WriteInfo("[New] Ready for a new script.\n");
    }

    public void SetConsoleWriter(ConsoleWriter console) => _console = console;

    private void AppendConsole(string text, ConsoleColor color = ConsoleColor.Gray) => _console?.Write(text, color);

    private async Task OpenAsync()
    {
        var window = GetMainWindow();
        var files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false,
            Title = "Open Embers script",
            FileTypeFilter =
            [
                new FilePickerFileType("Embers scripts")
                {
                    Patterns = ["*.emb", "*.ers", "*.rs", "*.rb"]
                },
                new FilePickerFileType("Any")
                {
                    Patterns = ["*.*"]
                }
            ]
        });

        if (files.Count == 0) return;

        var file = files[0];
        _currentFile = file.Path.LocalPath;
        EditorText = await File.ReadAllTextAsync(_currentFile);
        _console?.WriteInfo($"[Opened] ");
        _console?.WriteLine(_currentFile, ConsoleColor.Cyan);
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(_currentFile))
        {
            var window = GetMainWindow();
            var file = await window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save Embers script",
                DefaultExtension = "emb",
                FileTypeChoices =
                [
                    new FilePickerFileType("Embers scripts")
                    {
                        Patterns = ["*.emb", "*.ers", "*.rs", "*.rb"]
                    },
                    new FilePickerFileType("Any")
                    {
                        Patterns = ["*.*"]
                    }
                ]
            });

            if (file is null) return;
            _currentFile = file.Path.LocalPath;
        }

        await File.WriteAllTextAsync(_currentFile!, EditorText);
        _console?.WriteSuccess($"[Saved] ");
        _console?.WriteLine(_currentFile, ConsoleColor.Cyan);
    }

    private async Task AddReferenceAsync()
    {
        var window = GetMainWindow();
        var files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = true,
            Title = "Add DLL reference",
            FileTypeFilter =
            [
                new FilePickerFileType("Assemblies")
                {
                    Patterns = ["*.dll"]
                },
                new FilePickerFileType("Any")
                {
                    Patterns = ["*.*"]
                }
            ]
        });

        if (files.Count == 0) return;

        foreach (var file in files)
        {
            var path = file.Path.LocalPath;
            try
            {
                _host.AddReferenceAssembly(path);
                if (!ReferenceAssemblies.Contains(path))
                    ReferenceAssemblies.Add(path);
                _console?.WriteSuccess("[Reference added] ");
                _console?.WriteLine(path, ConsoleColor.Cyan);
            }
            catch (Exception ex)
            {
                _console?.WriteError("[Reference error] ");
                _console?.WriteLine($"{path} - {ex.Message}", ConsoleColor.Red);
            }
        }

        RefreshFunctionLists();
    }

    private void RemoveReference(object? parameter)
    {
        var path = parameter as string;
        if (string.IsNullOrWhiteSpace(path)) return;

        if (!ReferenceAssemblies.Remove(path)) return;

        _host.RemoveReferenceAssembly(path);
        _console?.WriteSuccess("[Reference removed] ");
        _console?.WriteLine(path, ConsoleColor.Cyan);

        RefreshFunctionLists();
    }

    private async Task OpenHelpAsync()
    {
        var owner = GetMainWindow();
        var dialog = new FunctionHelpDialog();
        dialog.DataContext = new FunctionHelpViewModel(() => dialog.Close());
        await dialog.ShowDialog(owner);
    }

    private void ToggleRightPanel() => IsRightPanelVisible = !IsRightPanelVisible;

    private void RefreshFunctionLists()
    {
        var (stdLib, _) = FunctionScanner.ScanFunctionNames();
        var hostAssemblies = new List<Assembly> { Assembly.GetExecutingAssembly() };
        hostAssemblies.AddRange(_host.GetReferenceAssembliesSnapshot());
        var host = FunctionScanner.ScanHostFunctionNames(hostAssemblies);

        _allStdLibFunctions.Clear();
        foreach (var name in stdLib)
            _allStdLibFunctions.Add(new FunctionListItem(name, LookupComment(name)));

        _allHostFunctions.Clear();
        foreach (var name in host)
            _allHostFunctions.Add(new FunctionListItem(name, LookupComment(name)));

        ApplyFunctionFilters();
    }

    private void ApplyFunctionFilters()
    {
        ApplyFunctionFilter(_allStdLibFunctions, StdLibFunctions, _stdLibFilterText);
        ApplyFunctionFilter(_allHostFunctions, HostFunctions, _hostFilterText);
    }

    private static void ApplyFunctionFilter(
        IReadOnlyList<FunctionListItem> source,
        ObservableCollection<FunctionListItem> target,
        string filter)
    {
        var needle = filter?.Trim();
        target.Clear();

        if (string.IsNullOrWhiteSpace(needle))
        {
            foreach (var item in source)
                target.Add(item);
            return;
        }

        foreach (var item in source)
        {
            if (item.Name.Contains(needle, StringComparison.OrdinalIgnoreCase))
                target.Add(item);
        }
    }

    private static string LookupComment(string name)
    {
        if (!FunctionScanner.TryGetFunctionDocumentation(name, out var doc))
            return string.Empty;

        return doc.Comments ?? string.Empty;
    }

    private void AddWhitelistEntry()
    {
        var entry = NewWhitelistEntry?.Trim();
        if (string.IsNullOrWhiteSpace(entry))
            return;

        if (WhitelistEntries.Contains(entry))
        {
            _console?.WriteWarning($"[Whitelist] Entry already exists: {entry}\n");
            return;
        }

        WhitelistEntries.Add(entry);
        NewWhitelistEntry = string.Empty;

        if (IsWhitelistMode)
            ApplySecurityPolicy();
    }

    private void RemoveWhitelistEntry()
    {
        if (string.IsNullOrWhiteSpace(SelectedWhitelistEntry))
            return;

        WhitelistEntries.Remove(SelectedWhitelistEntry);
        SelectedWhitelistEntry = null;

        if (IsWhitelistMode)
            ApplySecurityPolicy();
    }

    private void ClearWhitelist()
    {
        WhitelistEntries.Clear();
        if (IsWhitelistMode)
            ApplySecurityPolicy();
    }

    private void ApplySecurityPolicy()
    {
        _host.SetSecurityPolicy(SelectedSecurityMode, WhitelistEntries);
        _console?.WriteInfo($"[Security] Mode set to {SelectedSecurityMode}.\n");
    }

    private async Task RunAsync() => await RunTextAsync(EditorText, "Executing script...");

    private async Task RunTextAsync(string source, string statusMessage)
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        _console?.WritePrompt("[Run] ");
        _console?.WriteLine(statusMessage, ConsoleColor.Yellow);
        try
        {
            var result = await _host.ExecuteAsync(source, _cts.Token);
            if (result is not null)
            {
                _console?.WriteSuccess("=> ");
                _console?.WriteLine(result.ToString() ?? string.Empty, ConsoleColor.Green);
            }
        }
        catch (OperationCanceledException)
        {
            _console?.WriteWarning("[Cancelled]\n");
        }
        catch (Exception ex)
        {
            _console?.WriteError("[Error] ");
            _console?.WriteLine(ex.Message, ConsoleColor.Red);
        }
    }

    private async Task RunSelectionAsync(object? parameter)
    {
        var selection = parameter as string;
        if (string.IsNullOrWhiteSpace(selection))
        {
            _console?.WriteWarning("[Run] No selection to execute.\n");
            return;
        }

        await RunTextAsync(selection, "Executing selection...");
    }

    private void Stop()
    {
        _cts?.Cancel();
        _console?.WriteWarning("[Stop] Execution cancelled\n");
    }

    private static Window GetMainWindow()
    {
        // Avalonia classic: get main window from current app lifetime.
        var lifetime = (Avalonia.Application.Current!.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)!;
        return lifetime.MainWindow!;
    }
}
