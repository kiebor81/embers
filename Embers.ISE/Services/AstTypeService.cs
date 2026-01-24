using Embers.Compiler.Parsing;
using Embers.Expressions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Embers.ISE.Services;

internal static class AstTypeService
{
    private static int _lastHash;
    private static IReadOnlyList<TypeDefinition> _lastTypes = [];

    public static IReadOnlyList<TypeDefinition> GetTypeDefinitions(string text)
    {
        if (string.IsNullOrEmpty(text))
            return [];

        int hash = HashCode.Combine(text.Length, text.GetHashCode());
        if (hash == _lastHash)
            return _lastTypes;

        var types = BuildTypeDefinitions(text, out _);
        _lastHash = hash;
        _lastTypes = types;
        return types;
    }

    public static bool TryGetTypeDefinitions(string text, out IReadOnlyList<TypeDefinition> types, out string? errorMessage)
    {
        types = [];
        errorMessage = null;

        if (string.IsNullOrEmpty(text))
            return true;

        types = BuildTypeDefinitions(text, out errorMessage);
        return errorMessage == null;
    }

    private static IReadOnlyList<TypeDefinition> BuildTypeDefinitions(string text, out string? errorMessage)
    {
        var commands = TryParseCommands(text, out errorMessage);
        if (commands.Count == 0)
            return [];

        var collector = new TypeCollector();
        collector.Collect(commands);
        return collector.GetTypeDefinitions();
    }

    private static List<IExpression> TryParseCommands(string text, out string? errorMessage)
    {
        return Parser.TryParseCommands(text, out errorMessage);
    }

    internal enum TypeKind
    {
        Class,
        Module
    }

    internal readonly record struct TypeDefinition(
        string Name,
        TypeKind Kind,
        IReadOnlyList<string> InstanceMethods,
        IReadOnlyList<string> ClassMethods,
        IReadOnlyList<string> IncludedModules);

    private sealed class TypeScope(string name, TypeKind kind)
    {
        private readonly HashSet<string> _instanceMethods = new(StringComparer.Ordinal);
        private readonly HashSet<string> _classMethods = new(StringComparer.Ordinal);
        private readonly List<string> _includedModules = [];

        public string Name { get; } = name;
        public TypeKind Kind { get; } = kind;

        public void AddInstanceMethod(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
                _instanceMethods.Add(name);
        }

        public void AddClassMethod(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
                _classMethods.Add(name);
        }

        public void AddIncludedModule(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;

            if (!_includedModules.Contains(name))
                _includedModules.Add(name);
        }

        public TypeDefinition ToDefinition()
            => new(Name, Kind, [.. _instanceMethods], [.. _classMethods], [.. _includedModules]);
    }

    private sealed class TypeCollector
    {
        private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        private readonly Stack<TypeScope> _types = new();
        private readonly List<TypeDefinition> _definitions = [];

        public void Collect(IReadOnlyList<IExpression> commands)
        {
            foreach (var command in commands)
                Visit(command);
        }

        public IReadOnlyList<TypeDefinition> GetTypeDefinitions() => _definitions;

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
                case ClassExpression classExpr:
                    HandleClassExpression(classExpr);
                    return;
                case ModuleExpression moduleExpr:
                    HandleModuleExpression(moduleExpr);
                    return;
                case DefExpression def:
                    HandleDefExpression(def);
                    return;
                case CallExpression call:
                    HandleCallExpression(call);
                    return;
            }

            VisitChildren(expression);
        }

        private void HandleClassExpression(ClassExpression classExpr)
        {
            var named = GetFieldValue<INamedExpression>(classExpr, "namedexpression");
            if (named == null || string.IsNullOrWhiteSpace(named.Name))
                return;

            var scope = new TypeScope(named.Name, TypeKind.Class);
            _types.Push(scope);
            Visit(GetFieldValue<IExpression>(classExpr, "expression"));
            _types.Pop();
            _definitions.Add(scope.ToDefinition());
        }

        private void HandleModuleExpression(ModuleExpression moduleExpr)
        {
            var name = GetFieldValue<string>(moduleExpr, "name");
            if (string.IsNullOrWhiteSpace(name))
                return;

            var scope = new TypeScope(name, TypeKind.Module);
            _types.Push(scope);
            Visit(GetFieldValue<IExpression>(moduleExpr, "expression"));
            _types.Pop();
            _definitions.Add(scope.ToDefinition());
        }

        private void HandleDefExpression(DefExpression def)
        {
            if (_types.Count == 0)
                return;

            var named = GetFieldValue<INamedExpression>(def, "namedexpression");
            if (named == null || string.IsNullOrWhiteSpace(named.Name))
                return;

            var scope = _types.Peek();
            var target = named.TargetExpression;
            if (target == null)
            {
                scope.AddInstanceMethod(named.Name);
                return;
            }

            if (IsSelfTarget(target) || MatchesTypeName(target, scope.Name))
            {
                scope.AddClassMethod(named.Name);
                return;
            }

            // Ignore defs on other targets (best-effort).
        }

        private void HandleCallExpression(CallExpression call)
        {
            if (_types.Count == 0)
                return;

            var name = GetFieldValue<string>(call, "name");
            if (!string.Equals(name, "include", StringComparison.Ordinal))
                return;

            var args = GetFieldValue<IList<IExpression>>(call, "arguments");
            if (args == null)
                return;

            foreach (var arg in args)
            {
                var moduleName = GetTypeReferenceName(arg);
                if (!string.IsNullOrWhiteSpace(moduleName))
                    _types.Peek().AddIncludedModule(moduleName);
            }
        }

        private static bool IsSelfTarget(IExpression target) => target is SelfExpression;

        private static bool MatchesTypeName(IExpression target, string typeName)
        {
            if (target is NameExpression nameExpr)
                return string.Equals(nameExpr.Name, typeName, StringComparison.Ordinal);

            if (target is DoubleColonExpression dcExpr)
            {
                var qualified = dcExpr.AsQualifiedName();
                if (!string.IsNullOrWhiteSpace(qualified))
                    return string.Equals(ExtractLastQualifiedSegment(qualified), typeName, StringComparison.Ordinal);

                return string.Equals(dcExpr.Name, typeName, StringComparison.Ordinal);
            }

            return false;
        }

        private static string? GetTypeReferenceName(IExpression expression)
        {
            return expression switch
            {
                NameExpression nameExpr => nameExpr.Name,
                DoubleColonExpression dcExpr => ExtractLastQualifiedSegment(dcExpr.AsQualifiedName() ?? dcExpr.Name),
                _ => null
            };
        }

        private static string ExtractLastQualifiedSegment(string qualified)
        {
            if (string.IsNullOrWhiteSpace(qualified))
                return qualified;

            var parts = qualified.Split(["::"], StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 0 ? parts[^1] : qualified;
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
