using Embers.Annotations;
using Embers.Compiler.Parsing;
using Embers.Exceptions;
using Embers.Expressions;
using Embers.Language.Primitive;
using Embers.StdLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Text;

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

    private static readonly Regex SimpleCallRegex =
        new(@"^(?<name>[a-z_][A-Za-z0-9_]*[!?]?)\s*(\(|$)", RegexOptions.Compiled);

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

        var completionStopwatch = Stopwatch.StartNew();

        if (caretOffset < 0 || caretOffset > text.Length)
            caretOffset = text.Length;

        var context = AnalyzeContext(text, caretOffset);
        if (context.IsInString || context.IsInComment)
            return [];

        var results = new Dictionary<string, CompletionCandidate>(StringComparer.OrdinalIgnoreCase);
        bool hasKnownMemberType = false;
        var cacheHit = CompletionAnalysisCache.TryGetSnapshot(text, out var analysisSnapshot);
        var returnMaps = cacheHit
            ? analysisSnapshot.ReturnMaps
            : AstReturnTypeService.GetMethodReturnMaps(text);

        if (context.IsMemberAccess)
        {
            var typeTable = cacheHit
                ? new Dictionary<string, string>(analysisSnapshot.TypeTable, StringComparer.OrdinalIgnoreCase)
                : BuildTypeTable(text, caretOffset, returnMaps, allowAst: false);
            var (targetType, isClassAccess) = InferMemberAccessTypeInfo(text, caretOffset, typeTable);
            hasKnownMemberType = !string.IsNullOrWhiteSpace(targetType);
            if (hasKnownMemberType)
            {
                if (WorkspaceSymbolIndex.TryGetTypeDefinition(targetType!, out _))
                {
                    var projectMethods = WorkspaceSymbolIndex.GetTypeMethods(targetType!, isClassAccess);
                    foreach (var method in projectMethods)
                        AddCandidate(results, method, CandidateRank.ProjectMemberMethod);
                }

                if (!isClassAccess)
                {
                    var typedMethods = GetMethodNamesForType(targetType);
                    foreach (var method in typedMethods)
                        AddCandidate(results, method, CandidateRank.StdLibMemberMethod);
                }
            }
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

        IReadOnlyList<string> output;
        if (string.IsNullOrEmpty(context.Prefix))
        {
            output = [.. filtered.OrderBy(candidate => candidate.Rank)
                                 .ThenBy(candidate => candidate.Name, StringComparer.OrdinalIgnoreCase)
                                 .Select(candidate => candidate.Name)];
        }
        else
        {
            var prefix = context.Prefix;
            output =
            [
                .. filtered.OrderBy(candidate => candidate.Name.StartsWith(prefix, StringComparison.Ordinal) ? 0 : 1)
                           .ThenBy(candidate => candidate.Rank)
                           .ThenBy(candidate => candidate.Name, StringComparer.OrdinalIgnoreCase)
                           .Select(candidate => candidate.Name)
            ];
        }

        completionStopwatch.Stop();
        CompletionAnalysisCache.RecordCompletion(completionStopwatch.Elapsed, cacheHit);
        return output;
    }

    public static IReadOnlyList<string> GetCompletions(string text, int caretOffset, out int replaceStart)
    {
        replaceStart = caretOffset;

        if (text == null)
            return [];

        if (caretOffset < 0 || caretOffset > text.Length)
            caretOffset = text.Length;

        var prefix = ExtractIdentifierPrefix(text, caretOffset);
        replaceStart = string.IsNullOrEmpty(prefix)
            ? caretOffset
            : Math.Max(0, caretOffset - prefix.Length);

        return GetCompletions(text, caretOffset);
    }

    public static string GetInferenceTrace(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "Inference trace: empty buffer.";

        var trace = new StringBuilder();
        var returnMaps = AstReturnTypeService.GetMethodReturnMaps(text);

        trace.AppendLine("Inference trace");
        AppendReturnMapTrace(trace, returnMaps);

        var table = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var steps = new List<string>();
        if (TryBuildTypeTableFromAst(text, returnMaps, out table, steps))
        {
            trace.AppendLine();
            trace.AppendLine("Assignments");
            foreach (var line in steps)
                trace.AppendLine(line);

            trace.AppendLine();
            trace.AppendLine("Final inferred types");
            foreach (var entry in table.OrderBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase))
                trace.AppendLine($"{entry.Key} => {entry.Value}");
        }
        else
        {
            trace.AppendLine();
            trace.AppendLine("Inference trace: AST parse failed, no assignment trace available.");
        }

        return trace.ToString();
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

    private static Dictionary<string, string> BuildTypeTable(
        string text,
        int caretOffset,
        AstReturnTypeService.MethodReturnMaps returnMaps,
        bool allowAst = true)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string snippet = text[..Math.Min(caretOffset, text.Length)];
        var lines = snippet.Split('\n');

        int startLine = Math.Max(0, lines.Length - MaxTypeScanLines);
        var trimmedSnippet = startLine == 0 ? snippet : string.Join('\n', lines[startLine..]);
        if (allowAst && TryBuildTypeTableFromAst(trimmedSnippet, returnMaps, out var astTable))
            return astTable;

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

            var inferred = InferTypeFromExpression(expr, result, returnMaps);
            if (!string.IsNullOrEmpty(inferred))
                result[name] = inferred;
        }

        return result;
    }

    internal static Dictionary<string, string> BuildTypeTableSnapshot(
        string text,
        AstReturnTypeService.MethodReturnMaps returnMaps)
    {
        if (TryBuildTypeTableFromAst(text, returnMaps, out var table))
            return table;

        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    private static bool TryBuildTypeTableFromAst(
        string text,
        AstReturnTypeService.MethodReturnMaps returnMaps,
        out Dictionary<string, string> table)
    {
        table = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var commands = TryParseCommands(text);
        if (commands.Count == 0)
            return false;

        var visitor = new TypeInferenceVisitor(table, returnMaps);
        visitor.Visit(commands);
        return true;
    }

    private static bool TryBuildTypeTableFromAst(
        string text,
        AstReturnTypeService.MethodReturnMaps returnMaps,
        out Dictionary<string, string> table,
        List<string> trace)
    {
        table = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var commands = TryParseCommands(text);
        if (commands.Count == 0)
            return false;

        var visitor = new TypeInferenceVisitor(table, returnMaps, trace);
        visitor.Visit(commands);
        return true;
    }

    private static List<IExpression> TryParseCommands(string text)
    {
        var commands = new List<IExpression>();
        var parser = new Parser(text);

        try
        {
            for (var command = parser.ParseCommand(); command != null; command = parser.ParseCommand())
                commands.Add(command);
        }
        catch (SyntaxError)
        {
            // Best-effort: return successfully parsed commands.
        }

        return commands;
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

    private static string? InferTypeFromExpression(
        string expr,
        Dictionary<string, string> typeTable,
        AstReturnTypeService.MethodReturnMaps returnMaps)
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

        var chainType = TryInferTypeFromChain(expr, typeTable, returnMaps);
        if (!string.IsNullOrWhiteSpace(chainType))
            return chainType;

        var simpleCall = TryGetSimpleCallName(expr);
        if (!string.IsNullOrWhiteSpace(simpleCall)
            && returnMaps.GlobalMethods.TryGetValue(simpleCall, out var methodReturnType))
            return methodReturnType;

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

    private static (string? TypeName, bool IsClassAccess) InferMemberAccessTypeInfo(
        string text,
        int caretOffset,
        Dictionary<string, string> typeTable)
    {
        var target = ExtractMemberAccessTarget(text, caretOffset);
        if (string.IsNullOrWhiteSpace(target))
            return (null, false);

        if (typeTable.TryGetValue(target, out var inferred))
            return (inferred, false);

        if (target.StartsWith('"') || target.StartsWith('\''))
            return ("String", false);

        if (target.StartsWith(':'))
            return ("Symbol", false);

        if (NumericRegex.IsMatch(target))
            return (target.Contains('.') ? "Float" : "Fixnum", false);

        if (QualifiedConstantRegex.IsMatch(target))
            return (ExtractLastQualifiedSegment(target), true);

        return (null, false);
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

    private static string? TryGetSimpleCallName(string expr)
    {
        var match = SimpleCallRegex.Match(expr);
        if (!match.Success)
            return null;

        return match.Groups["name"].Value;
    }

    private static string? TryInferTypeFromChain(
        string expr,
        Dictionary<string, string> typeTable,
        AstReturnTypeService.MethodReturnMaps returnMaps)
    {
        if (string.IsNullOrWhiteSpace(expr) || !expr.Contains('.'))
            return null;

        if (!TrySplitDotChain(expr, out var baseToken, out var methods))
            return null;

        var (baseType, isClassAccess) = InferBaseTypeInfo(baseToken, typeTable, returnMaps);
        if (string.IsNullOrWhiteSpace(baseType))
            return null;

        foreach (var method in methods)
        {
            if (string.IsNullOrWhiteSpace(method))
                return null;

            if (isClassAccess && string.Equals(method, "new", StringComparison.Ordinal))
            {
                isClassAccess = false;
                continue;
            }

            if (!returnMaps.TypeMethods.TryGetValue(baseType, out var typeMap))
                return null;

            var map = isClassAccess ? typeMap.ClassMethods : typeMap.InstanceMethods;
            if (!map.TryGetValue(method, out var nextType))
                return null;

            baseType = nextType;
            isClassAccess = false;
        }

        return baseType;
    }

    private static (string? TypeName, bool IsClassAccess) InferBaseTypeInfo(
        string token,
        Dictionary<string, string> typeTable,
        AstReturnTypeService.MethodReturnMaps returnMaps)
    {
        var expr = token.Trim();
        if (expr.Length == 0)
            return (null, false);

        if (typeTable.TryGetValue(expr, out var existing))
            return (existing, false);

        if (expr.StartsWith('"') || expr.StartsWith('\''))
            return ("String", false);

        if (expr.StartsWith(':'))
            return ("Symbol", false);

        if (NumericRegex.IsMatch(expr))
            return (expr.Contains('.') ? "Float" : "Fixnum", false);

        var simpleCall = TryGetSimpleCallName(expr);
        if (!string.IsNullOrWhiteSpace(simpleCall)
            && returnMaps.GlobalMethods.TryGetValue(simpleCall, out var methodReturnType))
            return (methodReturnType, false);

        if (QualifiedConstantRegex.IsMatch(expr))
            return (ExtractLastQualifiedSegment(expr), true);

        return (null, false);
    }

    private static bool TrySplitDotChain(string expr, out string baseToken, out List<string> methods)
    {
        baseToken = string.Empty;
        methods = [];

        var parts = expr.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length < 2)
            return false;

        baseToken = parts[0];
        if (NumericRegex.IsMatch(baseToken)
            && parts.Length > 1
            && parts[1].Length > 0
            && char.IsDigit(parts[1][0]))
            return false;

        for (int i = 1; i < parts.Length; i++)
        {
            var method = ExtractLeadingIdentifier(parts[i]);
            if (string.IsNullOrWhiteSpace(method))
                return false;

            methods.Add(method);
        }

        return true;
    }

    private static string? ExtractLeadingIdentifier(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        int i = 0;
        while (i < text.Length && char.IsWhiteSpace(text[i]))
            i++;

        if (i >= text.Length || !(char.IsLetter(text[i]) || text[i] == '_'))
            return null;

        int start = i;
        i++;
        while (i < text.Length)
        {
            char ch = text[i];
            if (char.IsLetterOrDigit(ch) || ch == '_' || ch == '?' || ch == '!')
            {
                i++;
                continue;
            }
            break;
        }

        return text[start..i];
    }

    private static void AppendReturnMapTrace(
        StringBuilder trace,
        AstReturnTypeService.MethodReturnMaps returnMaps)
    {
        trace.AppendLine("Return type maps");
        if (returnMaps.GlobalMethods.Count == 0 && returnMaps.TypeMethods.Count == 0)
        {
            trace.AppendLine("  (none)");
            return;
        }

        if (returnMaps.GlobalMethods.Count > 0)
        {
            trace.AppendLine("  Global methods");
            foreach (var entry in returnMaps.GlobalMethods.OrderBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase))
                trace.AppendLine($"    {entry.Key} => {entry.Value}");
        }

        foreach (var entry in returnMaps.TypeMethods.OrderBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase))
        {
            trace.AppendLine($"  {entry.Key}");
            var typeMap = entry.Value;

            if (typeMap.InstanceMethods.Count > 0)
            {
                trace.AppendLine("    instance");
                foreach (var method in typeMap.InstanceMethods.OrderBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase))
                    trace.AppendLine($"      {method.Key} => {method.Value}");
            }

            if (typeMap.ClassMethods.Count > 0)
            {
                trace.AppendLine("    class");
                foreach (var method in typeMap.ClassMethods.OrderBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase))
                    trace.AppendLine($"      {method.Key} => {method.Value}");
            }
        }
    }

    private sealed class TypeInferenceVisitor(
        Dictionary<string, string> table,
        AstReturnTypeService.MethodReturnMaps returnMaps,
        List<string>? trace = null)
    {
        private readonly Dictionary<string, string> _table = table;
        private readonly AstReturnTypeService.MethodReturnMaps _returnMaps = returnMaps;
        private readonly List<string>? _trace = trace;

        public void Visit(IReadOnlyList<IExpression> commands)
        {
            foreach (var command in commands)
                Visit(command);
        }

        private void Visit(IExpression? expression)
        {
            if (expression == null)
                return;

            switch (expression)
            {
                case CompositeExpression composite:
                    foreach (var command in composite.Commands)
                        Visit(command);
                    return;
                case AssignExpression assign:
                    ApplyAssignment(assign.Name, assign.Expression);
                    return;
                case AssignInstanceVarExpression assignInstance:
                    ApplyAssignment(assignInstance.Name, assignInstance.Expression);
                    return;
                case AssignClassVarExpression assignClass:
                    ApplyAssignment(assignClass.Name, assignClass.Expression);
                    return;
                case AssignGlobalVarExpression assignGlobal:
                    ApplyAssignment(assignGlobal.Name, assignGlobal.Expression);
                    return;
                case IfExpression ifExpr:
                    HandleIfExpression(ifExpr);
                    return;
                case UnlessExpression unlessExpr:
                    HandleUnlessExpression(unlessExpr);
                    return;
                case TernaryExpression ternary:
                    HandleTernaryExpression(ternary);
                    return;
            }

            VisitChildren(expression);
        }

        private void ApplyAssignment(string name, IExpression expression)
        {
            var (typeName, _) = InferTypeInfo(expression);
            if (!string.IsNullOrWhiteSpace(typeName))
            {
                _table[name] = typeName;
                _trace?.Add($"{name} => {typeName}");
            }
        }

        private void HandleIfExpression(IfExpression ifExpr)
        {
            var thenExpr = GetFieldValue<IExpression>(ifExpr, "thencommand");
            var elseExpr = GetFieldValue<IExpression>(ifExpr, "elsecommand");
            MergeConditional(thenExpr, elseExpr);
        }

        private void HandleUnlessExpression(UnlessExpression unlessExpr)
        {
            var thenExpr = GetFieldValue<IExpression>(unlessExpr, "thenBlock");
            var elseExpr = GetFieldValue<IExpression>(unlessExpr, "elseBlock");
            MergeConditional(thenExpr, elseExpr);
        }

        private void HandleTernaryExpression(TernaryExpression ternary)
        {
            var trueExpr = GetFieldValue<IExpression>(ternary, "trueExpr");
            var falseExpr = GetFieldValue<IExpression>(ternary, "falseExpr");
            MergeConditional(trueExpr, falseExpr);
        }

        private void MergeConditional(IExpression? thenExpr, IExpression? elseExpr)
        {
            if (thenExpr == null || elseExpr == null)
                return;

            var before = new Dictionary<string, string>(_table, StringComparer.OrdinalIgnoreCase);
            var thenTable = new Dictionary<string, string>(before, StringComparer.OrdinalIgnoreCase);
            var elseTable = new Dictionary<string, string>(before, StringComparer.OrdinalIgnoreCase);

            var thenVisitor = new TypeInferenceVisitor(thenTable, _returnMaps);
            var elseVisitor = new TypeInferenceVisitor(elseTable, _returnMaps);
            thenVisitor.Visit(thenExpr);
            elseVisitor.Visit(elseExpr);

            foreach (var (key, thenValue) in thenTable)
            {
                if (!elseTable.TryGetValue(key, out var elseValue))
                    continue;

                if (!string.Equals(thenValue, elseValue, StringComparison.OrdinalIgnoreCase))
                    continue;

                _table[key] = thenValue;
                _trace?.Add($"{key} => {thenValue} (merged if/else)");
            }
        }

        private (string? TypeName, bool IsClassAccess) InferTypeInfo(IExpression expression)
        {
            if (expression is ConstantExpression constant)
                return (MapConstantType(constant.Value), false);

            if (expression is ArrayExpression)
                return ("Array", false);

            if (expression is HashExpression)
                return ("Hash", false);

            if (expression is InterpolatedStringExpression)
                return ("String", false);

            if (expression is RegexLiteralExpression)
                return ("Regexp", false);

            if (expression is RangeExpression)
                return ("Range", false);

            if (expression is NameExpression nameExpr)
            {
                if (_table.TryGetValue(nameExpr.Name, out var existing))
                    return (existing, false);

                if (_returnMaps.GlobalMethods.TryGetValue(nameExpr.Name, out var methodType))
                    return (methodType, false);

                if (!string.IsNullOrWhiteSpace(nameExpr.Name) && char.IsUpper(nameExpr.Name[0]))
                    return (nameExpr.Name, true);

                return (null, false);
            }

            if (expression is DoubleColonExpression dcExpr)
            {
                var qualified = dcExpr.AsQualifiedName();
                var name = ExtractLastQualifiedSegment(qualified ?? dcExpr.Name);
                return (name, true);
            }

            if (expression is CallExpression call)
            {
                var callName = GetFieldValue<string>(call, "name");
                if (!string.IsNullOrWhiteSpace(callName)
                    && _returnMaps.GlobalMethods.TryGetValue(callName, out var methodType))
                    return (methodType, false);

                return (null, false);
            }

            if (expression is DotExpression dot)
            {
                var (receiverType, receiverIsClass) = InferTypeInfo(dot.TargetExpression);
                if (string.IsNullOrWhiteSpace(receiverType))
                    return (null, false);

                if (receiverIsClass && string.Equals(dot.Name, "new", StringComparison.Ordinal))
                    return (receiverType, false);

                if (!_returnMaps.TypeMethods.TryGetValue(receiverType, out var typeMap))
                    return (null, false);

                var map = receiverIsClass ? typeMap.ClassMethods : typeMap.InstanceMethods;
                if (map.TryGetValue(dot.Name, out var methodType))
                    return (methodType, false);

                return (null, false);
            }

            return (null, false);
        }

        private static string MapConstantType(object value)
        {
            if (value == null)
                return "NilClass";

            return value switch
            {
                string => "String",
                bool b => b ? "TrueClass" : "FalseClass",
                int or long or short or byte => "Fixnum",
                double or float or decimal => "Float",
                Symbol => "Symbol",
                DateTime => "DateTime",
                _ => value.GetType().Name
            };
        }

        private static T? GetFieldValue<T>(object instance, string fieldName)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            var field = instance.GetType().GetField(fieldName, flags);
            if (field == null)
                return default;

            var value = field.GetValue(instance);
            if (value is T typed)
                return typed;

            return default;
        }

        private void VisitChildren(IExpression expression)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            foreach (var field in expression.GetType().GetFields(flags))
            {
                if (field.IsStatic)
                    continue;

                var value = field.GetValue(expression);
                if (value == null || value is string)
                    continue;

                if (value is IExpression child)
                {
                    Visit(child);
                    continue;
                }

                if (value is IEnumerable<IExpression> typedChildren)
                {
                    foreach (var item in typedChildren)
                        Visit(item);
                    continue;
                }

                if (value is IEnumerable enumerable)
                {
                    foreach (var item in enumerable)
                        if (item is IExpression expr)
                            Visit(expr);
                }
            }
        }
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
            _cachedStdLibFunctionNames = [.. stdLib];
            _cachedHostFunctionNames = [.. host];
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
        ProjectMemberMethod = 0,
        StdLibMemberMethod = 1,
        Local = 2,
        Method = 3,
        Constant = 4,
        WorkspaceConstant = 5,
        StdLibFunction = 6,
        HostFunction = 7,
        Keyword = 8,
        Identifier = 9
    }

    private readonly record struct CompletionContext(
        bool IsInString,
        bool IsInComment,
        bool IsMemberAccess,
        bool IsNamespaceAccess,
        string Prefix);
}
