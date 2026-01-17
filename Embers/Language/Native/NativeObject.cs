using Embers.Functions;

namespace Embers.Language.Native;

/// <summary>
/// DynamicObject wrapper for native CLR values to enable DynamicClass dispatch.
/// </summary>
public sealed class NativeObject : DynamicObject
{
    public NativeObject(NativeClassAdapter nativeClass, object? nativeValue)
        : base(nativeClass)
    {
        NativeClass = nativeClass ?? throw new ArgumentNullException(nameof(nativeClass));
        NativeValue = nativeValue;
    }

    public NativeClassAdapter NativeClass { get; }

    public object? NativeValue { get; }

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
