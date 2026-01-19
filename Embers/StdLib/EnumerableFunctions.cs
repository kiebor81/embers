using System.Collections;
using Embers.Exceptions;
using Embers.Functions;

namespace Embers.StdLib.Enumerable;

/// <summary>
/// Helper methods for enumerable functions.
/// </summary>
internal static class EnumerableHelpers
{
    /// <summary>
    /// Enumerates the elements of a dynamic object.
    /// </summary>
    /// <param name="self">The dynamic object to enumerate.</param>
    /// <param name="context">The current context.</param>
    /// <param name="methodName">The name of the calling method (for error messages).</param>
    /// <returns>An enumerable of argument lists.</returns>
    public static IEnumerable<IList<object>> Enumerate(DynamicObject self, Context context, string methodName)
    {
        if (self is NativeObject nativeObject)
            return EnumerateNative(nativeObject.NativeValue, methodName);

        if (self is IEnumerable enumerable)
            return EnumerateEnumerable(enumerable);

        var each = self.GetMethod("each");
        if (each == null)
            throw new TypeError($"{methodName} expects an enumerable");

        var collected = new List<IList<object>>();
        var collector = new YieldCollector(collected);

        if (each is ICallableWithBlock callable)
        {
            callable.ApplyWithBlock(self, context, Array.Empty<object>(), collector);
            return collected;
        }

        var applyWithBlock = each.GetType().GetMethod("ApplyWithBlock");
        if (applyWithBlock == null)
            throw new TypeError("each expects a block");

        applyWithBlock.Invoke(each, [self, context, Array.Empty<object>(), collector]);
        return collected;
    }

    /// <summary>
    /// Determines whether a dynamic object is a dictionary.
    /// </summary>
    /// <param name="self">The dynamic object to check.</param>
    /// <returns>True if the object is a dictionary; otherwise, false.</returns>
    public static bool IsDictionary(DynamicObject self)
    {
        if (self is NativeObject nativeObject)
            return nativeObject.NativeValue is IDictionary;

        return self is IDictionary;
    }

    /// <summary>
    /// Converts a list of arguments to a single element.
    /// </summary>
    /// <param name="args">The list of arguments.</param>
    /// <returns>The single element or a pair.</returns>
    public static object? AsElement(IList<object> args)
    {
        if (args.Count == 0)
            return null;

        if (args.Count == 1)
            return args[0];

        var pair = new DynamicArray();
        foreach (var arg in args)
            pair.Add(arg);
        return pair;
    }

    /// <summary>
    /// Unwraps a dynamic object to its native value if applicable.
    /// </summary>
    /// <param name="self">The dynamic object to unwrap.</param>
    /// <returns>The native value or the original object.</returns>
    public static object UnwrapSelf(DynamicObject self) =>
        self is NativeObject nativeObject ? nativeObject.NativeValue ?? self : self;

    /// <summary>
    /// Enumerates a native enumerable.
    /// </summary>
    /// <param name="value">The native value to enumerate.</param>
    /// <param name="methodName">The name of the calling method (for error messages).</param>
    /// <returns>An enumerable of argument lists.</returns>
    private static IEnumerable<IList<object>> EnumerateNative(object? value, string methodName)
    {
        if (value == null || value is string)
            throw new TypeError($"{methodName} expects an enumerable");

        if (value is IDictionary dictionary)
        {
            foreach (DictionaryEntry entry in dictionary)
                yield return new List<object> { entry.Key, entry.Value };
            yield break;
        }

        if (value is IEnumerable enumerable)
        {
            foreach (var item in enumerable)
                yield return new List<object> { item };
            yield break;
        }

        throw new TypeError($"{methodName} expects an enumerable");
    }

    /// <summary>
    /// Enumerates a standard enumerable.
    /// </summary>
    /// <param name="enumerable">The enumerable to enumerate.</param>
    /// <returns>An enumerable of argument lists.</returns>
    private static IEnumerable<IList<object>> EnumerateEnumerable(IEnumerable enumerable)
    {
        foreach (var item in enumerable)
            yield return new List<object> { item };
    }

    /// <summary>
    /// Collector for yielded values.
    /// </summary>
    private sealed class YieldCollector : IFunction
    {
        private readonly List<IList<object>> items;

        public YieldCollector(List<IList<object>> items) => this.items = items;

        public object Apply(DynamicObject self, Context context, IList<object> values)
        {
            var captured = new List<object>();
            if (values != null)
                foreach (var value in values)
                    captured.Add(value);
            items.Add(captured);
            return null;
        }
    }
}

/// <summary>
/// Enumerable map function.
/// </summary>
public sealed class EnumerableMapFunction : StdFunction
{
    public override object? Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (context.Block == null)
            throw new ArgumentError("map expects a block");

        var result = new DynamicArray();
        foreach (var args in EnumerableHelpers.Enumerate(self, context, "map"))
            result.Add(context.Block.Apply(context.Self, context, args));

        return result;
    }
}

/// <summary>
/// Enumerable select function.
/// </summary>
public sealed class EnumerableSelectFunction : StdFunction
{
    public override object? Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (context.Block == null)
            throw new ArgumentError("select expects a block");

        var isDictionary = EnumerableHelpers.IsDictionary(self);
        if (isDictionary)
        {
            var result = new DynamicHash();
            foreach (var args in EnumerableHelpers.Enumerate(self, context, "select"))
            {
                var blockResult = context.Block.Apply(context.Self, context, args);
                if (Predicates.IsTrue(blockResult))
                {
                    var key = args.Count > 0 ? args[0] : null;
                    var value = args.Count > 1 ? args[1] : null;
                    if (key != null)
                        result[key] = value;
                }
            }
            return result;
        }

        var arrayResult = new DynamicArray();
        foreach (var args in EnumerableHelpers.Enumerate(self, context, "select"))
        {
            var blockResult = context.Block.Apply(context.Self, context, args);
            if (Predicates.IsTrue(blockResult))
                arrayResult.Add(EnumerableHelpers.AsElement(args));
        }

        return arrayResult;
    }
}

/// <summary>
/// Enumerable reject function.
/// </summary>
public sealed class EnumerableRejectFunction : StdFunction
{
    public override object? Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (context.Block == null)
            throw new ArgumentError("reject expects a block");

        var isDictionary = EnumerableHelpers.IsDictionary(self);
        if (isDictionary)
        {
            var result = new DynamicHash();
            foreach (var args in EnumerableHelpers.Enumerate(self, context, "reject"))
            {
                var blockResult = context.Block.Apply(context.Self, context, args);
                if (!Predicates.IsTrue(blockResult))
                {
                    var key = args.Count > 0 ? args[0] : null;
                    var value = args.Count > 1 ? args[1] : null;
                    if (key != null)
                        result[key] = value;
                }
            }
            return result;
        }

        var arrayResult = new DynamicArray();
        foreach (var args in EnumerableHelpers.Enumerate(self, context, "reject"))
        {
            var blockResult = context.Block.Apply(context.Self, context, args);
            if (!Predicates.IsTrue(blockResult))
                arrayResult.Add(EnumerableHelpers.AsElement(args));
        }

        return arrayResult;
    }
}

/// <summary>
/// Enumerable each_with_index function.
/// </summary>
public sealed class EnumerableEachWithIndexFunction : StdFunction
{
    public override object? Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (context.Block == null)
            throw new ArgumentError("each_with_index expects a block");

        long index = 0;
        foreach (var args in EnumerableHelpers.Enumerate(self, context, "each_with_index"))
        {
            var blockArgs = new List<object>(args) { index };
            context.Block.Apply(context.Self, context, blockArgs);
            index++;
        }

        return EnumerableHelpers.UnwrapSelf(self);
    }
}

/// <summary>
/// Enumerable reduce function.
/// </summary>
public sealed class EnumerableReduceFunction : StdFunction
{
    public override object? Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (context.Block == null)
            throw new ArgumentError("reduce expects a block");

        var items = EnumerableHelpers.Enumerate(self, context, "reduce");
        using var enumerator = items.GetEnumerator();

        object? accumulator;
        if (values.Count > 0)
        {
            accumulator = values[0];
        }
        else
        {
            if (!enumerator.MoveNext())
                return null;

            accumulator = EnumerableHelpers.AsElement(enumerator.Current);
        }

        while (enumerator.MoveNext())
        {
            var args = new List<object> { accumulator };
            foreach (var value in enumerator.Current)
                args.Add(value);

            accumulator = context.Block.Apply(context.Self, context, args);
        }

        return accumulator;
    }
}

/// <summary>
/// Enumerable any? function.
/// </summary>
public sealed class EnumerableAnyFunction : StdFunction
{
    public override object? Apply(DynamicObject self, Context context, IList<object> values)
    {
        var items = EnumerableHelpers.Enumerate(self, context, "any?");

        if (context.Block != null)
        {
            foreach (var args in items)
            {
                if (Predicates.IsTrue(context.Block.Apply(context.Self, context, args)))
                    return true;
            }

            return false;
        }

        foreach (var args in items)
            if (Predicates.IsTrue(EnumerableHelpers.AsElement(args)))
                return true;

        return false;
    }
}

/// <summary>
/// Enumerable all? function.
/// </summary>
public sealed class EnumerableAllFunction : StdFunction
{
    public override object? Apply(DynamicObject self, Context context, IList<object> values)
    {
        var items = EnumerableHelpers.Enumerate(self, context, "all?");

        if (context.Block != null)
        {
            foreach (var args in items)
                if (!Predicates.IsTrue(context.Block.Apply(context.Self, context, args)))
                    return false;

            return true;
        }

        foreach (var args in items)
            if (!Predicates.IsTrue(EnumerableHelpers.AsElement(args)))
                return false;

        return true;
    }
}

/// <summary>
/// Enumerable find function.
/// </summary>
public sealed class EnumerableFindFunction : StdFunction
{
    public override object? Apply(DynamicObject self, Context context, IList<object> values)
    {
        var items = EnumerableHelpers.Enumerate(self, context, "find");

        if (context.Block != null)
        {
            foreach (var args in items)
            {
                if (Predicates.IsTrue(context.Block.Apply(context.Self, context, args)))
                    return EnumerableHelpers.AsElement(args);
            }

            return null;
        }

        foreach (var args in items)
        {
            var element = EnumerableHelpers.AsElement(args);
            if (Predicates.IsTrue(element))
                return element;
        }

        return null;
    }
}

/// <summary>
/// Enumerable to_a function.
/// </summary>
public sealed class EnumerableToAFunction : StdFunction
{
    public override object? Apply(DynamicObject self, Context context, IList<object> values)
    {
        var result = new DynamicArray();
        foreach (var args in EnumerableHelpers.Enumerate(self, context, "to_a"))
            result.Add(EnumerableHelpers.AsElement(args));

        return result;
    }
}
