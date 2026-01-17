using Embers.Language.Primitive;
using System.Collections;

namespace Embers.Language.Native;

/// <summary>
/// Resolves CLR values to their NativeClassAdapter instances.
/// </summary>
public static class NativeClassResolver
{
    public static NativeClassAdapter? Resolve(Context context, object? value)
    {
        var root = context.RootContext;

        if (value == null)
            return root.GetValue("NilClass") as NativeClassAdapter;

        if (value is int || value is long || value is short || value is byte)
            return root.GetValue("Fixnum") as NativeClassAdapter;

        if (value is double || value is float || value is decimal)
            return root.GetValue("Float") as NativeClassAdapter;

        if (value is string)
            return root.GetValue("String") as NativeClassAdapter;

        if (value is Symbol)
            return root.GetValue("Symbol") as NativeClassAdapter;

        if (value is DateTime)
            return root.GetValue("DateTime") as NativeClassAdapter;

        if (value is bool boolValue)
            return root.GetValue(boolValue ? "TrueClass" : "FalseClass") as NativeClassAdapter;

        if (value is IDictionary)
            return root.GetValue("Hash") as NativeClassAdapter;

        if (value is IList)
            return root.GetValue("Array") as NativeClassAdapter;

        if (value is Proc)
            return root.GetValue("Proc") as NativeClassAdapter;

        if (value is Primitive.Range)
            return root.GetValue("Range") as NativeClassAdapter;

        return null;
    }
}
