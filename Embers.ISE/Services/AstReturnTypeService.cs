using Embers.Compiler.Parsing;
using Embers.Exceptions;
using Embers.Expressions;
using Embers.Language.Primitive;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Embers.ISE.Services;

internal static class AstReturnTypeService
{
    private static int _lastHash;
    private static MethodReturnMaps _lastMaps = new(
        new Dictionary<string, string>(StringComparer.Ordinal),
        new Dictionary<string, TypeMethodReturnMap>(StringComparer.Ordinal));

    public static MethodReturnMaps GetMethodReturnMaps(string text)
    {
        if (string.IsNullOrEmpty(text))
            return EmptyMaps();

        int hash = HashCode.Combine(text.Length, text.GetHashCode());
        if (hash == _lastHash)
            return _lastMaps;

        var maps = BuildReturnMaps(text);
        _lastHash = hash;
        _lastMaps = maps;
        return maps;
    }

    private static MethodReturnMaps BuildReturnMaps(string text)
    {
        var commands = TryParseCommands(text, out _);
        if (commands.Count == 0)
            return EmptyMaps();

        var collector = new ReturnTypeCollector();
        collector.Collect(commands);
        return collector.GetMaps();
    }

    private static List<IExpression> TryParseCommands(string text, out string? errorMessage)
    {
        errorMessage = null;
        var commands = new List<IExpression>();
        var parser = new Parser(text);

        try
        {
            for (var command = parser.ParseCommand(); command != null; command = parser.ParseCommand())
                commands.Add(command);
        }
        catch (SyntaxError ex)
        {
            // Best-effort: return successfully parsed commands.
            errorMessage = ex.Message;
        }

        return commands;
    }

    internal readonly record struct MethodReturnMaps(
        IReadOnlyDictionary<string, string> GlobalMethods,
        IReadOnlyDictionary<string, TypeMethodReturnMap> TypeMethods);

    internal readonly record struct TypeMethodReturnMap(
        IReadOnlyDictionary<string, string> InstanceMethods,
        IReadOnlyDictionary<string, string> ClassMethods);

    private sealed class TypeScope(string name)
    {
        public string Name { get; } = name;
    }

    private sealed class MethodReturnBuilder
    {
        public Dictionary<string, string> InstanceMethods { get; } = new(StringComparer.Ordinal);
        public Dictionary<string, string> ClassMethods { get; } = new(StringComparer.Ordinal);
    }

    private sealed class ReturnTypeCollector
    {
        private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        private readonly Stack<TypeScope> _types = new();
        private readonly Dictionary<string, string> _globalMethods = new(StringComparer.Ordinal);
        private readonly Dictionary<string, MethodReturnBuilder> _typeMethods = new(StringComparer.Ordinal);

        public void Collect(IReadOnlyList<IExpression> commands)
        {
            foreach (var command in commands)
                Visit(command);
        }

        public MethodReturnMaps GetMaps()
        {
            var typeMaps = new Dictionary<string, TypeMethodReturnMap>(StringComparer.Ordinal);
            foreach (var (name, builder) in _typeMethods)
            {
                typeMaps[name] = new TypeMethodReturnMap(
                    builder.InstanceMethods,
                    builder.ClassMethods);
            }

            return new MethodReturnMaps(_globalMethods, typeMaps);
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
                case ClassExpression classExpr:
                    HandleClassExpression(classExpr);
                    return;
                case ModuleExpression moduleExpr:
                    HandleModuleExpression(moduleExpr);
                    return;
                case DefExpression def:
                    HandleDefExpression(def);
                    return;
            }

            VisitChildren(expression);
        }

        private void HandleClassExpression(ClassExpression classExpr)
        {
            var named = GetFieldValue<INamedExpression>(classExpr, "namedexpression");
            if (named == null || string.IsNullOrWhiteSpace(named.Name))
                return;

            _types.Push(new TypeScope(named.Name));
            Visit(GetFieldValue<IExpression>(classExpr, "expression"));
            _types.Pop();
        }

        private void HandleModuleExpression(ModuleExpression moduleExpr)
        {
            var name = GetFieldValue<string>(moduleExpr, "name");
            if (string.IsNullOrWhiteSpace(name))
                return;

            _types.Push(new TypeScope(name));
            Visit(GetFieldValue<IExpression>(moduleExpr, "expression"));
            _types.Pop();
        }

        private void HandleDefExpression(DefExpression def)
        {
            var named = GetFieldValue<INamedExpression>(def, "namedexpression");
            if (named == null || string.IsNullOrWhiteSpace(named.Name))
                return;

            var body = GetFieldValue<IExpression>(def, "expression");
            var returnType = InferReturnType(body);
            if (string.IsNullOrWhiteSpace(returnType))
                return;

            if (_types.Count == 0)
            {
                _globalMethods[named.Name] = returnType;
                return;
            }

            var typeName = _types.Peek().Name;
            if (!_typeMethods.TryGetValue(typeName, out var builder))
            {
                builder = new MethodReturnBuilder();
                _typeMethods[typeName] = builder;
            }

            if (IsClassMethodTarget(named.TargetExpression, typeName))
                builder.ClassMethods[named.Name] = returnType;
            else
                builder.InstanceMethods[named.Name] = returnType;
        }

        private static bool IsClassMethodTarget(IExpression? target, string typeName)
        {
            if (target == null)
                return false;

            if (target is SelfExpression)
                return true;

            if (target is NameExpression nameExpr)
                return string.Equals(nameExpr.Name, typeName, StringComparison.Ordinal);

            if (target is DoubleColonExpression dcExpr)
            {
                var qualified = dcExpr.AsQualifiedName();
                var name = ExtractLastQualifiedSegment(qualified ?? dcExpr.Name);
                return string.Equals(name, typeName, StringComparison.Ordinal);
            }

            return false;
        }

        private static string? InferReturnType(IExpression? expression)
        {
            if (expression == null)
                return null;

            if (expression is ReturnExpression ret)
                return InferReturnType(GetFieldValue<IExpression>(ret, "expression"));

            if (expression is CompositeExpression composite)
                return InferReturnType(composite.Commands.LastOrDefault());

            if (expression is ConstantExpression constant)
                return MapConstantType(constant.Value);

            if (expression is ArrayExpression)
                return "Array";

            if (expression is HashExpression)
                return "Hash";

            if (expression is InterpolatedStringExpression)
                return "String";

            if (expression is RegexLiteralExpression)
                return "Regexp";

            if (expression is RangeExpression)
                return "Range";

            if (expression is DotExpression dot && string.Equals(dot.Name, "new", StringComparison.Ordinal))
            {
                var typeName = ExtractTypeName(dot.TargetExpression);
                if (!string.IsNullOrWhiteSpace(typeName))
                    return typeName;
            }

            return null;
        }

        private static string? ExtractTypeName(IExpression? expression)
        {
            return expression switch
            {
                NameExpression nameExpr => nameExpr.Name,
                DoubleColonExpression dcExpr => ExtractLastQualifiedSegment(dcExpr.AsQualifiedName() ?? dcExpr.Name),
                _ => null
            };
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

    private static MethodReturnMaps EmptyMaps()
        => new(
            new Dictionary<string, string>(StringComparer.Ordinal),
            new Dictionary<string, TypeMethodReturnMap>(StringComparer.Ordinal));
}
