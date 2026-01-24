using Embers.Compiler.Parsing;
using Embers.Expressions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Embers.ISE.Services;

internal static class AstSymbolService
{
    private static int _lastHash;
    private static int _lastCaret;
    private static IReadOnlyList<SymbolEntry> _lastSymbols = [];

    public static IReadOnlyList<SymbolEntry> GetSymbols(string text, int caretOffset)
    {
        if (string.IsNullOrEmpty(text))
            return [];

        int hash = HashCode.Combine(text.Length, text.GetHashCode());
        if (hash == _lastHash && caretOffset == _lastCaret)
            return _lastSymbols;

        var symbols = BuildSymbols(text, caretOffset);
        _lastHash = hash;
        _lastCaret = caretOffset;
        _lastSymbols = symbols;
        return symbols;
    }

    public static bool TryGetWorkspaceSymbols(string text, out IReadOnlyList<SymbolEntry> symbols, out string? errorMessage)
    {
        symbols = [];
        errorMessage = null;

        if (string.IsNullOrEmpty(text))
            return true;

        var commands = TryParseCommands(text, out errorMessage);
        if (commands.Count == 0)
            return errorMessage == null;

        var collector = new SymbolCollector();
        collector.Collect(commands);
        symbols = collector.GetAllSymbols();
        return errorMessage == null;
    }

    private static IReadOnlyList<SymbolEntry> BuildSymbols(string text, int caretOffset)
    {
        if (caretOffset < 0 || caretOffset > text.Length)
            caretOffset = text.Length;

        var prefixText = text[..caretOffset];
        var commands = TryParseCommands(prefixText, out _);

        if (commands.Count == 0 && caretOffset < text.Length)
        {
            var fullCommands = TryParseCommands(text, out _);
            if (fullCommands.Count > 0)
                commands = fullCommands;
        }

        if (commands.Count == 0)
            return _lastSymbols.Count > 0 ? _lastSymbols : [];

        var collector = new SymbolCollector();
        collector.Collect(commands);
        return collector.GetVisibleSymbols();
    }

    private static List<IExpression> TryParseCommands(string text, out string? errorMessage)
    {
        return Parser.TryParseCommands(text, out errorMessage);
    }

    internal enum SymbolKind
    {
        Local,
        Method,
        Class,
        Module,
        Constant,
        Global,
        InstanceVar,
        ClassVar
    }

    internal readonly record struct SymbolEntry(string Name, SymbolKind Kind, ScopeKind Scope);

    private sealed class SymbolScope(SymbolScope? parent, ScopeKind kind)
    {
        private readonly Dictionary<string, SymbolEntry> _symbols = new(StringComparer.Ordinal);

        public SymbolScope? Parent { get; } = parent;
        public ScopeKind Kind { get; } = kind;
        public IEnumerable<SymbolEntry> Symbols => _symbols.Values;

        public void Add(SymbolEntry entry)
        {
            if (!_symbols.ContainsKey(entry.Name))
                _symbols[entry.Name] = entry;
        }
    }

    internal enum ScopeKind
    {
        Root,
        Class,
        Module,
        Method,
        Block
    }

    private sealed class SymbolCollector
    {
        private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        private readonly Stack<SymbolScope> _scopes = new();
        private readonly List<SymbolEntry> _allSymbols = [];

        public SymbolCollector() => _scopes.Push(new SymbolScope(null, ScopeKind.Root));

        public void Collect(IReadOnlyList<IExpression> commands)
        {
            foreach (var command in commands)
                Visit(command);
        }

        public IReadOnlyList<SymbolEntry> GetVisibleSymbols()
        {
            var result = new List<SymbolEntry>();
            var seen = new HashSet<string>(StringComparer.Ordinal);

            for (var scope = _scopes.Peek(); scope != null; scope = scope.Parent)
            {
                foreach (var entry in scope.Symbols)
                {
                    if (seen.Add(entry.Name))
                        result.Add(entry);
                }
            }

            return result;
        }

        public IReadOnlyList<SymbolEntry> GetAllSymbols() => _allSymbols;

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
                    AddLocalOrConstant(assign.Name);
                    Visit(assign.Expression);
                    return;
                case AssignInstanceVarExpression assignInstance:
                    AddSymbol(assignInstance.Name, SymbolKind.InstanceVar);
                    Visit(assignInstance.Expression);
                    return;
                case AssignClassVarExpression assignClass:
                    AddSymbol(assignClass.Name, SymbolKind.ClassVar);
                    Visit(assignClass.Expression);
                    return;
                case AssignGlobalVarExpression assignGlobal:
                    AddSymbol(assignGlobal.Name, SymbolKind.Global);
                    Visit(assignGlobal.Expression);
                    return;
                case DefExpression def:
                    HandleDefExpression(def);
                    return;
                case ClassExpression classExpr:
                    HandleClassExpression(classExpr);
                    return;
                case ModuleExpression moduleExpr:
                    HandleModuleExpression(moduleExpr);
                    return;
                case BlockExpression block:
                    HandleBlockExpression(block);
                    return;
                case ForInExpression forIn:
                    HandleForInExpression(forIn);
                    return;
            }

            VisitChildren(expression);
        }

        private void HandleDefExpression(DefExpression def)
        {
            var named = GetFieldValue<INamedExpression>(def, "namedexpression");
            if (named != null)
                AddSymbol(named.Name, SymbolKind.Method);

            PushScope(ScopeKind.Method);

            foreach (var param in def.Parameters)
                AddSymbol(param, SymbolKind.Local);

            if (!string.IsNullOrWhiteSpace(def.SplatParameterName))
                AddSymbol(def.SplatParameterName, SymbolKind.Local);

            if (!string.IsNullOrWhiteSpace(def.KwargsParameterName))
                AddSymbol(def.KwargsParameterName, SymbolKind.Local);

            if (!string.IsNullOrWhiteSpace(def.BlockParameterName))
                AddSymbol(def.BlockParameterName, SymbolKind.Local);

            var body = GetFieldValue<IExpression>(def, "expression");
            Visit(body);

            PopScope();
        }

        private void HandleClassExpression(ClassExpression classExpr)
        {
            var named = GetFieldValue<INamedExpression>(classExpr, "namedexpression");
            if (named != null)
                AddSymbol(named.Name, SymbolKind.Class);

            PushScope(ScopeKind.Class);
            var body = GetFieldValue<IExpression>(classExpr, "expression");
            Visit(body);
            PopScope();
        }

        private void HandleModuleExpression(ModuleExpression moduleExpr)
        {
            var name = GetFieldValue<string>(moduleExpr, "name");
            if (!string.IsNullOrWhiteSpace(name))
                AddSymbol(name, SymbolKind.Module);

            PushScope(ScopeKind.Module);
            var body = GetFieldValue<IExpression>(moduleExpr, "expression");
            Visit(body);
            PopScope();
        }

        private void HandleBlockExpression(BlockExpression block)
        {
            PushScope(ScopeKind.Block);

            foreach (var param in block.Parameters)
                AddSymbol(param, SymbolKind.Local);

            Visit(block.Body);
            PopScope();
        }

        private void HandleForInExpression(ForInExpression forIn)
        {
            var name = GetFieldValue<string>(forIn, "name");
            if (!string.IsNullOrWhiteSpace(name))
                AddSymbol(name, SymbolKind.Local);

            Visit(GetFieldValue<IExpression>(forIn, "expression"));
            Visit(GetFieldValue<IExpression>(forIn, "command"));
        }

        private void AddLocalOrConstant(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;

            AddSymbol(name, char.IsUpper(name[0]) ? SymbolKind.Constant : SymbolKind.Local);
        }

        private void AddSymbol(string name, SymbolKind kind)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;

            var entry = new SymbolEntry(name, kind, _scopes.Peek().Kind);
            _scopes.Peek().Add(entry);
            _allSymbols.Add(entry);
        }

        private void PushScope(ScopeKind kind) => _scopes.Push(new SymbolScope(_scopes.Peek(), kind));

        private void PopScope()
        {
            if (_scopes.Count > 1)
                _scopes.Pop();
        }

        private static T? GetFieldValue<T>(object instance, string fieldName)
        {
            var field = instance.GetType().GetField(fieldName, InstanceFlags);
            if (field == null)
                return default;

            var value = field.GetValue(instance);
            if (value is T typed)
                return typed;

            return default;
        }

        private void VisitChildren(IExpression expression)
        {
            foreach (var field in expression.GetType().GetFields(InstanceFlags))
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
}
