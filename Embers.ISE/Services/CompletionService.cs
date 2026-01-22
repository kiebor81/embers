using Embers.Annotations;
using Embers.StdLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

namespace Embers.ISE.Services;

public static class CompletionService
{
    private static readonly string[] Keywords =
    [
        "and", "begin", "break", "case", "class", "def", "do", "else", "elsif",
        "end", "ensure", "false", "for", "if", "in", "module", "next", "nil",
        "not", "or", "redo", "require", "rescue", "return", "self", "then",
        "true", "unless", "until", "when", "while", "yield"
    ];

    private static readonly Regex IdentifierRegex =
        new(@"@{0,2}[A-Za-z_][A-Za-z0-9_]*[!?]?", RegexOptions.Compiled);

    private static readonly Regex AssignmentRegex =
        new(@"^\s*(?<name>@{0,2}[A-Za-z_][A-Za-z0-9_]*)\s*=\s*(?<expr>.+)$", RegexOptions.Compiled);

    private static readonly Regex NumericRegex =
        new(@"^\d+(\.\d+)?\b", RegexOptions.Compiled);

    private static readonly Regex NumericTokenRegex =
        new(@"^\d+(\.\d+)?$", RegexOptions.Compiled);

    private static readonly Regex NewTypeRegex =
        new(@"^(?<type>[A-Z][A-Za-z0-9_]*(?:::[A-Z][A-Za-z0-9_]*)*)\s*\.\s*new\b", RegexOptions.Compiled);

    private static readonly Regex QualifiedConstantRegex =
        new(@"^[A-Z][A-Za-z0-9_]*(?:::[A-Z][A-Za-z0-9_]*)*$", RegexOptions.Compiled);

    private static IReadOnlyDictionary<string, HashSet<string>>? _stdLibMethodsByType;
    private static IReadOnlyList<string>? _cachedStdLibFunctionNames;
    private static IReadOnlyList<string>? _cachedHostFunctionNames;
    private static readonly Lock StdLibLock = new();
    private static readonly Lock FunctionCacheLock = new();
    private const int MaxTypeScanLines = 250;

    public static IReadOnlyList<string> GetCompletions(string text, int caretOffset)
    {
        if (text == null)
            return [];

        if (caretOffset < 0 || caretOffset > text.Length)
            caretOffset = text.Length;

        var context = AnalyzeContext(text, caretOffset);
        if (context.IsInString || context.IsInComment)
            return [];

        var results = new Dictionary<string, CompletionCandidate>(StringComparer.OrdinalIgnoreCase);
        bool hasKnownMemberType = false;

        if (context.IsMemberAccess)
        {
            var typeTable = BuildTypeTable(text, caretOffset);
            var targetType = InferMemberAccessType(text, caretOffset, typeTable);
            hasKnownMemberType = !string.IsNullOrWhiteSpace(targetType);
            var typedMethods = GetMethodNamesForType(targetType);
            foreach (var method in typedMethods)
                AddCandidate(results, method, CandidateRank.MemberMethod);
        }
        else
        {
            var symbols = AstSymbolService.GetSymbols(text, caretOffset);
            foreach (var symbol in symbols)
                AddCandidate(results, symbol.Name, GetSymbolRank(symbol.Kind));

            foreach (var symbol in WorkspaceSymbolIndex.GetWorkspaceSymbols())
                AddCandidate(results, symbol.Name, CandidateRank.WorkspaceConstant);
        }

        if (!context.IsMemberAccess)
        {
            var (stdLib, host) = GetFunctionNames();
            foreach (var name in stdLib) AddCandidate(results, name, CandidateRank.StdLibFunction);
            foreach (var name in host) AddCandidate(results, name, CandidateRank.HostFunction);
        }

        if (!context.IsMemberAccess && !context.IsNamespaceAccess)
        {
            foreach (var keyword in Keywords) AddCandidate(results, keyword, CandidateRank.Keyword);
        }

        if (!context.IsMemberAccess || (!hasKnownMemberType && results.Count == 0))
        {
            foreach (Match match in IdentifierRegex.Matches(text))
            {
                if (match.Success && !string.IsNullOrWhiteSpace(match.Value))
                    AddCandidate(results, match.Value, CandidateRank.Identifier);
            }
        }

        IEnumerable<CompletionCandidate> filtered = results.Values;

        if (context.IsMemberAccess)
            filtered = filtered.Where(candidate => ShouldIncludeForMemberAccess(candidate.Name));
        else if (context.IsNamespaceAccess)
            filtered = filtered.Where(candidate => ShouldIncludeForNamespaceAccess(candidate.Name));

        if (!string.IsNullOrEmpty(context.Prefix))
            filtered = filtered.Where(candidate => candidate.Name.StartsWith(context.Prefix, StringComparison.OrdinalIgnoreCase));

        if (string.IsNullOrEmpty(context.Prefix))
            return [.. filtered.OrderBy(candidate => candidate.Rank)
                               .ThenBy(candidate => candidate.Name, StringComparer.OrdinalIgnoreCase)
                               .Select(candidate => candidate.Name)];

        var prefix = context.Prefix;
        return
        [
            .. filtered.OrderBy(candidate => candidate.Name.StartsWith(prefix, StringComparison.Ordinal) ? 0 : 1)
                       .ThenBy(candidate => candidate.Rank)
                       .ThenBy(candidate => candidate.Name, StringComparer.OrdinalIgnoreCase)
                       .Select(candidate => candidate.Name)
        ];
    }

    private static CompletionContext AnalyzeContext(string text, int caretOffset)
    {
        bool inSingle = false;
        bool inDouble = false;
        bool inComment = false;
        bool escape = false;

        for (int i = 0; i < caretOffset; i++)
        {
            char ch = text[i];

            if (inComment)
            {
                if (ch == '\n')
                    inComment = false;
                continue;
            }

            if (inSingle)
            {
                if (escape)
                {
                    escape = false;
                    continue;
                }

                if (ch == '\\')
                {
                    escape = true;
                    continue;
                }

                if (ch == '\'')
                    inSingle = false;

                continue;
            }

            if (inDouble)
            {
                if (escape)
                {
                    escape = false;
                    continue;
                }

                if (ch == '\\')
                {
                    escape = true;
                    continue;
                }

                if (ch == '"')
                    inDouble = false;

                continue;
            }

            if (ch == '#')
            {
                inComment = true;
                continue;
            }

            if (ch == '\'')
            {
                inSingle = true;
                continue;
            }

            if (ch == '"')
            {
                inDouble = true;
            }
        }

        bool isMemberAccess = caretOffset > 0
            && text[caretOffset - 1] == '.'
            && !IsNumericLiteralBeforeDot(text, caretOffset - 1);
        bool isNamespaceAccess = caretOffset > 1
            && text[caretOffset - 2] == ':'
            && text[caretOffset - 1] == ':';

        string prefix = ExtractIdentifierPrefix(text, caretOffset);

        return new CompletionContext(inSingle || inDouble, inComment, isMemberAccess, isNamespaceAccess, prefix);
    }

    private static Dictionary<string, string> BuildTypeTable(string text, int caretOffset)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string snippet = text[..Math.Min(caretOffset, text.Length)];
        var lines = snippet.Split('\n');

        int startLine = Math.Max(0, lines.Length - MaxTypeScanLines);
        bool inSingle = false;
        bool inDouble = false;
        bool escape = false;

        for (int index = startLine; index < lines.Length; index++)
        {
            bool lineStartsInString = inSingle || inDouble;
            var line = StripLineComment(lines[index], ref inSingle, ref inDouble, ref escape);
            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (lineStartsInString)
                continue;

            var match = AssignmentRegex.Match(line);
            if (!match.Success)
                continue;

            var name = match.Groups["name"].Value;
            var expr = match.Groups["expr"].Value.Trim();

            var inferred = InferTypeFromExpression(expr, result);
            if (!string.IsNullOrEmpty(inferred))
                result[name] = inferred;
        }

        return result;
    }

    private static string StripLineComment(string line, ref bool inSingle, ref bool inDouble, ref bool escape)
    {
        for (int i = 0; i < line.Length; i++)
        {
            char ch = line[i];

            if (inSingle)
            {
                if (escape)
                {
                    escape = false;
                    continue;
                }

                if (ch == '\\')
                {
                    escape = true;
                    continue;
                }

                if (ch == '\'')
                    inSingle = false;

                continue;
            }

            if (inDouble)
            {
                if (escape)
                {
                    escape = false;
                    continue;
                }

                if (ch == '\\')
                {
                    escape = true;
                    continue;
                }

                if (ch == '"')
                    inDouble = false;

                continue;
            }

            if (ch == '#')
                return line[..i];

            if (ch == '\'')
            {
                inSingle = true;
                continue;
            }

            if (ch == '"')
            {
                inDouble = true;
            }
        }

        return line;
    }

    private static string? InferTypeFromExpression(string expr, Dictionary<string, string> typeTable)
    {
        if (string.IsNullOrWhiteSpace(expr))
            return null;

        if (expr.StartsWith('"') || expr.StartsWith('\''))
            return "String";

        if (expr.StartsWith(':'))
            return "Symbol";

        if (expr.StartsWith('['))
            return "Array";

        if (expr.StartsWith('{'))
            return "Hash";

        if (expr.StartsWith("true", StringComparison.Ordinal) && IsWordBoundary(expr, 4))
            return "TrueClass";

        if (expr.StartsWith("false", StringComparison.Ordinal) && IsWordBoundary(expr, 5))
            return "FalseClass";

        if (expr.StartsWith("nil", StringComparison.Ordinal) && IsWordBoundary(expr, 3))
            return "NilClass";

        var numericMatch = NumericRegex.Match(expr);
        if (numericMatch.Success && numericMatch.Index == 0)
        {
            return expr.Contains('.') ? "Float" : "Fixnum";
        }

        if (expr.Contains("..", StringComparison.Ordinal))
            return "Range";

        var newTypeMatch = NewTypeRegex.Match(expr);
        if (newTypeMatch.Success)
        {
            var typeName = ExtractLastQualifiedSegment(newTypeMatch.Groups["type"].Value);
            return typeName;
        }

        if (typeTable.TryGetValue(expr, out var existing))
            return existing;

        if (QualifiedConstantRegex.IsMatch(expr))
            return ExtractLastQualifiedSegment(expr);

        return null;
    }

    private static bool IsWordBoundary(string text, int indexAfter)
    {
        if (indexAfter >= text.Length)
            return true;

        return !char.IsLetterOrDigit(text[indexAfter]) && text[indexAfter] != '_';
    }

    private static string? InferMemberAccessType(string text, int caretOffset, Dictionary<string, string> typeTable)
    {
        var target = ExtractMemberAccessTarget(text, caretOffset);
        if (string.IsNullOrWhiteSpace(target))
            return null;

        if (typeTable.TryGetValue(target, out var inferred))
            return inferred;

        if (target.StartsWith('"') || target.StartsWith('\''))
            return "String";

        if (target.StartsWith(':'))
            return "Symbol";

        if (NumericRegex.IsMatch(target))
            return target.Contains('.') ? "Float" : "Fixnum";

        if (QualifiedConstantRegex.IsMatch(target))
            return ExtractLastQualifiedSegment(target);

        return null;
    }

    private static string? ExtractMemberAccessTarget(string text, int caretOffset)
    {
        if (caretOffset <= 0)
            return null;

        int dotIndex = caretOffset - 1;
        if (dotIndex < 0 || text[dotIndex] != '.')
            return null;

        int i = dotIndex - 1;
        while (i >= 0 && char.IsWhiteSpace(text[i]))
            i--;

        if (i < 0)
            return null;

        char ch = text[i];
        if (ch == '"' || ch == '\'')
            return ch.ToString();

        if (ch == ']' || ch == '}' || ch == ')')
            return null;

        int end = i;
        while (i >= 0)
        {
            ch = text[i];
            if (char.IsLetterOrDigit(ch) || ch == '_' || ch == '@' || ch == ':')
            {
                i--;
                continue;
            }
            break;
        }

        int start = i + 1;
        if (start > end)
            return null;

        return text.Substring(start, end - start + 1);
    }

    private static bool IsNumericLiteralBeforeDot(string text, int dotIndex)
    {
        if (dotIndex <= 0)
            return false;

        int i = dotIndex - 1;
        while (i >= 0 && (char.IsDigit(text[i]) || text[i] == '.'))
            i--;

        int start = i + 1;
        if (start >= dotIndex)
            return false;

        if (start > 0)
        {
            char before = text[start - 1];
            if (char.IsLetter(before) || before == '_' || before == '@')
                return false;
        }

        var token = text.Substring(start, dotIndex - start);
        return NumericTokenRegex.IsMatch(token);
    }

    private static string ExtractLastQualifiedSegment(string qualified)
    {
        if (string.IsNullOrWhiteSpace(qualified))
            return qualified;

        var parts = qualified.Split(["::"], StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 ? parts[^1] : qualified;
    }

    private static IReadOnlyCollection<string> GetMethodNamesForType(string? typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
            return [];

        var map = GetStdLibMethodMap();
        return map.TryGetValue(typeName, out var methods)
            ? methods
            : Array.Empty<string>();
    }

    private static IReadOnlyDictionary<string, HashSet<string>> GetStdLibMethodMap()
    {
        if (_stdLibMethodsByType != null)
            return _stdLibMethodsByType;

        lock (StdLibLock)
        {
            if (_stdLibMethodsByType != null)
                return _stdLibMethodsByType;

            var map = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types.Where(t => t != null).ToArray()!;
                }
                catch
                {
                    continue;
                }

                foreach (var type in types)
                {
                    if (type == null || !type.IsClass || type.IsAbstract)
                        continue;

                    var attr = type.GetCustomAttribute<StdLibAttribute>(inherit: false);
                    if (attr == null || attr.Names.Length == 0)
                        continue;

                    if (attr.TargetTypes == null || attr.TargetTypes.Length == 0)
                        continue;

                    foreach (var targetType in attr.TargetTypes)
                    {
                        if (string.IsNullOrWhiteSpace(targetType))
                            continue;

                        if (!map.TryGetValue(targetType, out var methods))
                        {
                            methods = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                            map[targetType] = methods;
                        }

                        foreach (var name in attr.Names)
                        {
                            if (!string.IsNullOrWhiteSpace(name))
                                methods.Add(name);
                        }
                    }
                }
            }

            _stdLibMethodsByType = map;
            return _stdLibMethodsByType;
        }
    }

    private static (IReadOnlyList<string> StdLib, IReadOnlyList<string> Host) GetFunctionNames()
    {
        if (_cachedStdLibFunctionNames != null && _cachedHostFunctionNames != null)
            return (_cachedStdLibFunctionNames, _cachedHostFunctionNames);

        lock (FunctionCacheLock)
        {
            if (_cachedStdLibFunctionNames != null && _cachedHostFunctionNames != null)
                return (_cachedStdLibFunctionNames, _cachedHostFunctionNames);

            var (stdLib, host) = FunctionScanner.ScanFunctionNames();
            _cachedStdLibFunctionNames = stdLib.ToArray();
            _cachedHostFunctionNames = host.ToArray();
            return (_cachedStdLibFunctionNames, _cachedHostFunctionNames);
        }
    }

    public static void RefreshCaches()
    {
        lock (FunctionCacheLock)
        {
            _cachedStdLibFunctionNames = null;
            _cachedHostFunctionNames = null;
        }

        lock (StdLibLock)
        {
            _stdLibMethodsByType = null;
        }
    }

    private static void AddCandidate(Dictionary<string, CompletionCandidate> candidates, string name, CandidateRank rank)
    {
        if (string.IsNullOrWhiteSpace(name))
            return;

        if (candidates.TryGetValue(name, out var existing))
        {
            if ((int)rank < (int)existing.Rank)
                candidates[name] = new CompletionCandidate(name, rank);
            return;
        }

        candidates[name] = new CompletionCandidate(name, rank);
    }

    private static CandidateRank GetSymbolRank(AstSymbolService.SymbolKind kind)
        => kind switch
        {
            AstSymbolService.SymbolKind.Local => CandidateRank.Local,
            AstSymbolService.SymbolKind.InstanceVar => CandidateRank.Local,
            AstSymbolService.SymbolKind.ClassVar => CandidateRank.Local,
            AstSymbolService.SymbolKind.Global => CandidateRank.Local,
            AstSymbolService.SymbolKind.Method => CandidateRank.Method,
            AstSymbolService.SymbolKind.Class => CandidateRank.Constant,
            AstSymbolService.SymbolKind.Module => CandidateRank.Constant,
            AstSymbolService.SymbolKind.Constant => CandidateRank.Constant,
            _ => CandidateRank.Identifier
        };

    private static string ExtractIdentifierPrefix(string text, int caretOffset)
    {
        if (caretOffset <= 0)
            return string.Empty;

        int i = caretOffset - 1;
        bool hasChars = false;

        while (i >= 0)
        {
            char ch = text[i];
            if (char.IsLetterOrDigit(ch) || ch == '_' || ch == '?' || ch == '!' || ch == '@')
            {
                hasChars = true;
                i--;
                continue;
            }

            if (ch == ':')
            {
                if (i > 0 && text[i - 1] == ':')
                    break;

                hasChars = true;
                i--;
                break;
            }

            break;
        }

        if (!hasChars)
            return string.Empty;

        int start = i + 1;
        if (start >= caretOffset)
            return string.Empty;

        string prefix = text[start..caretOffset];
        if (prefix.Length == 0)
            return string.Empty;

        int nameStart = 0;
        if (prefix[0] == ':')
            nameStart = 1;

        while (nameStart < prefix.Length && prefix[nameStart] == '@')
            nameStart++;

        if (nameStart < prefix.Length && char.IsDigit(prefix[nameStart]))
            return string.Empty;

        return prefix;
    }

    private static bool ShouldIncludeForMemberAccess(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        return !name.StartsWith('@');
    }

    private static bool ShouldIncludeForNamespaceAccess(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        if (name.StartsWith('@'))
            return false;

        return char.IsUpper(name[0]);
    }

    private readonly record struct CompletionCandidate(string Name, CandidateRank Rank);

    private enum CandidateRank
    {
        MemberMethod = 0,
        Local = 1,
        Method = 2,
        Constant = 3,
        WorkspaceConstant = 4,
        StdLibFunction = 5,
        HostFunction = 6,
        Keyword = 7,
        Identifier = 8
    }

    private readonly record struct CompletionContext(
        bool IsInString,
        bool IsInComment,
        bool IsMemberAccess,
        bool IsNamespaceAccess,
        string Prefix);
}
