using Embers.ISE.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Embers.ISE.Services;

internal static class WorkspaceSymbolIndex
{
    private static readonly ConcurrentDictionary<string, FileIndexEntry> Files = new(StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentDictionary<string, CancellationTokenSource> DebounceTokens = new(StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentDictionary<int, string> TabKeys = new();
    private static readonly ConcurrentDictionary<string, byte> ParsingKeys = new(StringComparer.OrdinalIgnoreCase);
    private static readonly object ProjectScanLock = new();
    private static readonly object IndexLock = new();
    private static CancellationTokenSource? _projectScanCts;
    private static string? _rootDirectory;
    private static string? _activeFileKey;

    private static readonly string[] ScriptExtensions = [".rb", ".emb", ".ers", ".rs"];
    private static readonly string[] ExcludedDirectories = ["bin", "obj", ".git", ".vs"];
    private const int DebounceMs = 200;
    private const bool IncludeInteropTypes = false;
    private static readonly Regex RequireRegex =
        new(@"^\s*require\s+['""](?<path>[^'""]+)['""]", RegexOptions.Compiled | RegexOptions.Multiline);
    private static readonly Dictionary<string, List<WorkspaceSymbol>> GlobalIndex = new(StringComparer.Ordinal);

    public static void Initialize(string rootDirectory)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory))
            return;

        _rootDirectory = rootDirectory;
        if (IncludeInteropTypes)
            Debug.WriteLine("[WorkspaceSymbols] Interop type scanning enabled.");
        QueueProjectScan();
    }

    public static void UpdateOpenTab(DocumentTab tab, bool immediate = false)
    {
        if (tab == null)
            return;

        var tabId = RuntimeHelpers.GetHashCode(tab);
        var key = GetTabKey(tab);
        if (TabKeys.TryGetValue(tabId, out var previousKey) && !previousKey.Equals(key, StringComparison.OrdinalIgnoreCase))
            RemoveByKey(previousKey);

        TabKeys[tabId] = key;
        var text = tab.Text ?? string.Empty;
        var filePath = tab.FilePath;

        if (immediate)
        {
            QueueParseImmediate(key, text, filePath);
            return;
        }

        ScheduleParse(key, text, filePath);
    }

    public static void RemoveOpenTab(DocumentTab tab)
    {
        if (tab == null)
            return;

        var tabId = RuntimeHelpers.GetHashCode(tab);
        if (TabKeys.TryRemove(tabId, out var key))
            RemoveByKey(key);

        lock (IndexLock)
        {
            if (GlobalIndex.Count == 0)
                return;

            foreach (var list in GlobalIndex.Values)
                list.RemoveAll(symbol => symbol.FilePath.Equals(key, StringComparison.OrdinalIgnoreCase));
        }
    }

    public static void SetActiveFile(DocumentTab? tab)
    {
        _activeFileKey = tab == null ? null : GetTabKey(tab);
    }

    public static IReadOnlyDictionary<string, string> GetParseErrors()
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in Files)
        {
            if (!string.IsNullOrWhiteSpace(entry.Value.LastError))
                result[entry.Key] = entry.Value.LastError!;
        }

        return result;
    }

    public static IReadOnlyList<WorkspaceSymbol> GetDefinitions(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return [];

        lock (IndexLock)
        {
            return GlobalIndex.TryGetValue(name, out var list)
                ? list.ToList()
                : [];
        }
    }

    public static IReadOnlyList<WorkspaceSymbol> GetWorkspaceSymbols()
    {
        var result = new List<WorkspaceSymbol>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        var activeKey = _activeFileKey;
        if (!string.IsNullOrWhiteSpace(activeKey) && Files.TryGetValue(activeKey, out var activeEntry))
        {
            foreach (var symbol in activeEntry.Symbols)
                if (seen.Add(symbol.Name))
                    result.Add(symbol);
        }

        foreach (var entry in Files.Values)
        {
            if (!string.IsNullOrWhiteSpace(activeKey)
                && entry.FilePath.Equals(activeKey, StringComparison.OrdinalIgnoreCase))
                continue;

            foreach (var symbol in entry.Symbols)
                if (seen.Add(symbol.Name))
                    result.Add(symbol);
        }

        return result;
    }

    private static string GetTabKey(DocumentTab tab)
    {
        if (!string.IsNullOrWhiteSpace(tab.FilePath))
            return tab.FilePath;

        return $"tab:{RuntimeHelpers.GetHashCode(tab)}";
    }

    private static void RemoveByKey(string key)
    {
        Files.TryRemove(key, out _);
        if (DebounceTokens.TryRemove(key, out var token))
        {
            token.Cancel();
            token.Dispose();
        }

        lock (IndexLock)
        {
            if (GlobalIndex.Count == 0)
                return;

            foreach (var list in GlobalIndex.Values)
                list.RemoveAll(symbol => symbol.FilePath.Equals(key, StringComparison.OrdinalIgnoreCase));
        }
    }

    private static void QueueProjectScan()
    {
        lock (ProjectScanLock)
        {
            _projectScanCts?.Cancel();
            _projectScanCts?.Dispose();
            _projectScanCts = new CancellationTokenSource();
            _ = Task.Run(() => ScanProjectFilesAsync(_projectScanCts.Token));
        }
    }

    public static void RefreshProjectIndex() => QueueProjectScan();

    private static async Task ScanProjectFilesAsync(CancellationToken token)
    {
        var root = _rootDirectory;
        if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
            return;

        var files = EnumerateScriptFiles(root);
        foreach (var path in files)
        {
            if (token.IsCancellationRequested)
                return;

            try
            {
                var text = await File.ReadAllTextAsync(path, token);
                QueueParseImmediate(path, text, path);
            }
            catch
            {
                // Skip unreadable files.
            }
        }
    }

    private static IEnumerable<string> EnumerateScriptFiles(string root)
    {
        var pending = new Stack<string>();
        pending.Push(root);

        while (pending.Count > 0)
        {
            var current = pending.Pop();
            IEnumerable<string> directories;
            try
            {
                directories = Directory.EnumerateDirectories(current);
            }
            catch
            {
                continue;
            }

            foreach (var dir in directories)
            {
                var name = Path.GetFileName(dir);
                if (ExcludedDirectories.Any(excluded => name.Equals(excluded, StringComparison.OrdinalIgnoreCase)))
                    continue;

                pending.Push(dir);
            }

            IEnumerable<string> files;
            try
            {
                files = Directory.EnumerateFiles(current);
            }
            catch
            {
                continue;
            }

            foreach (var file in files)
            {
                var ext = Path.GetExtension(file);
                if (ScriptExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase))
                    yield return file;
            }
        }
    }

    private static void ScheduleParse(string key, string text, string? filePath)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;

        if (DebounceTokens.TryGetValue(key, out var existingToken))
        {
            existingToken.Cancel();
            existingToken.Dispose();
        }

        var cts = new CancellationTokenSource();
        DebounceTokens[key] = cts;
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(DebounceMs, cts.Token);
            }
            catch (TaskCanceledException)
            {
                return;
            }

            if (!cts.IsCancellationRequested)
                QueueParseImmediate(key, text, filePath);
        });
    }

    private static void QueueParseImmediate(string key, string text, string? filePath)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;

        if (DebounceTokens.TryRemove(key, out var token))
        {
            token.Cancel();
            token.Dispose();
        }

        if (!ParsingKeys.TryAdd(key, 0))
            return;

        var hash = text.GetHashCode();
        try
        {
            if (Files.TryGetValue(key, out var existing) && existing.TextHash == hash)
                return;

            string? errorMessage = null;
            if (!AstSymbolService.TryGetWorkspaceSymbols(text, out var symbols, out errorMessage))
                Debug.WriteLine($"[WorkspaceSymbols] Parse failed: {filePath ?? key} - {errorMessage}");

            var symbolList = symbols
                .Where(symbol => symbol.Kind is AstSymbolService.SymbolKind.Class
                              or AstSymbolService.SymbolKind.Module
                              or AstSymbolService.SymbolKind.Constant)
                .Select(symbol =>
                {
                    var location = FindSymbolLocation(text, symbol.Name);
                    return new WorkspaceSymbol(
                        symbol.Name,
                        symbol.Kind,
                        symbol.Scope,
                        filePath ?? key,
                        location.Line,
                        location.Column);
                })
                .ToList();

            UpdateIndexes(key, hash, symbolList, errorMessage);

            if (!string.IsNullOrWhiteSpace(filePath))
            {
                foreach (var requirePath in ExtractRequirePaths(text))
                {
                    var resolved = ResolveRequirePath(filePath, requirePath);
                    if (resolved != null)
                    {
                        try
                        {
                            QueueParseImmediate(resolved, File.ReadAllText(resolved), resolved);
                        }
                        catch
                        {
                            // Skip unreadable require files.
                        }
                    }
                }
            }
        }
        finally
        {
            ParsingKeys.TryRemove(key, out _);
        }
    }

    private static void UpdateIndexes(string key, int hash, IReadOnlyList<WorkspaceSymbol> symbols, string? errorMessage)
    {
        lock (IndexLock)
        {
            if (Files.TryGetValue(key, out var previous))
            {
                foreach (var list in GlobalIndex.Values)
                    list.RemoveAll(symbol => symbol.FilePath.Equals(key, StringComparison.OrdinalIgnoreCase));
            }

            foreach (var symbol in symbols)
            {
                if (!GlobalIndex.TryGetValue(symbol.Name, out var list))
                {
                    list = [];
                    GlobalIndex[symbol.Name] = list;
                }

                list.Add(symbol);
            }

            Files[key] = new FileIndexEntry(hash, symbols, errorMessage, key);
        }
    }

    private static IEnumerable<string> ExtractRequirePaths(string text)
    {
        foreach (Match match in RequireRegex.Matches(text))
        {
            if (!match.Success)
                continue;

            var path = match.Groups["path"].Value;
            if (!string.IsNullOrWhiteSpace(path))
                yield return path;
        }
    }

    private static string? ResolveRequirePath(string sourceFilePath, string requirePath)
    {
        if (string.IsNullOrWhiteSpace(requirePath))
            return null;

        var baseDir = Path.GetDirectoryName(sourceFilePath);
        var candidates = new List<string>();
        if (!string.IsNullOrWhiteSpace(baseDir))
            candidates.Add(Path.Combine(baseDir, requirePath));

        if (!string.IsNullOrWhiteSpace(_rootDirectory))
            candidates.Add(Path.Combine(_rootDirectory, requirePath));

        foreach (var candidate in candidates)
        {
            foreach (var resolved in ExpandExtensions(candidate))
            {
                if (File.Exists(resolved))
                    return resolved;
            }
        }

        return null;
    }

    private static IEnumerable<string> ExpandExtensions(string path)
    {
        if (Path.HasExtension(path))
        {
            yield return path;
            yield break;
        }

        foreach (var ext in ScriptExtensions)
            yield return path + ext;
    }

    private static (int Line, int Column) FindSymbolLocation(string text, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return (0, 0);

        var index = IndexOfSymbol(text, name);
        if (index < 0)
            return (0, 0);

        int line = 1;
        int column = 1;
        for (int i = 0; i < index; i++)
        {
            if (text[i] == '\n')
            {
                line++;
                column = 1;
            }
            else
            {
                column++;
            }
        }

        return (line, column);
    }

    private static int IndexOfSymbol(string text, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return -1;

        if (name.StartsWith('@') || name.StartsWith('$'))
            return text.IndexOf(name, StringComparison.Ordinal);

        int index = 0;
        while (index < text.Length)
        {
            index = text.IndexOf(name, index, StringComparison.Ordinal);
            if (index < 0)
                return -1;

            if (IsWordBoundary(text, index - 1) && IsWordBoundary(text, index + name.Length))
                return index;

            index += name.Length;
        }

        return -1;
    }

    private static bool IsWordBoundary(string text, int index)
    {
        if (index < 0 || index >= text.Length)
            return true;

        var ch = text[index];
        return !char.IsLetterOrDigit(ch) && ch != '_';
    }

    internal readonly record struct WorkspaceSymbol(
        string Name,
        AstSymbolService.SymbolKind Kind,
        AstSymbolService.ScopeKind Scope,
        string FilePath,
        int Line,
        int Column);

    private readonly record struct FileIndexEntry(
        int TextHash,
        IReadOnlyList<WorkspaceSymbol> Symbols,
        string? LastError,
        string FilePath);
}
