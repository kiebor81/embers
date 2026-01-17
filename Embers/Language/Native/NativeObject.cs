using Embers.Functions;

namespace Embers.Language.Native;

/// <summary>
/// DynamicObject wrapper for native CLR values to enable DynamicClass dispatch.
/// </summary>
public sealed class NativeObject(NativeClassAdapter nativeClass, object? nativeValue) : DynamicObject(nativeClass)
{
    public NativeClassAdapter NativeClass { get; } = nativeClass ?? throw new ArgumentNullException(nameof(nativeClass));

    public object? NativeValue { get; } = nativeValue;

    public override IFunction? GetMethod(string name)
    {
        var method = base.GetMethod(name);
        if (method != null)
            return method;

        return NativeClass.NativeClass.GetInstanceMethod(name) != null
            ? new NativeStdFunction(NativeClass.NativeClass, name)
            : null;
    }

    public override string ToString() => NativeValue?.ToString() ?? "nil";
}
