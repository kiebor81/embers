using Embers.Functions;

namespace Embers.Language.Native;

/// <summary>
/// DynamicClass wrapper for native types to allow reopening and method lookup.
/// </summary>
public sealed class NativeClassAdapter : DynamicClass
{
    public NativeClassAdapter(DynamicClass @class, string name, DynamicClass superclass, DynamicClass parent, NativeClass nativeClass)
        : base(@class, name, superclass, parent)
    {
        NativeClass = nativeClass ?? throw new ArgumentNullException(nameof(nativeClass));
    }

    public NativeClass NativeClass { get; }

    public override IFunction? GetMethod(string name)
    {
        var method = base.GetMethod(name);
        if (method != null)
            return method;

        return NativeClass.GetMethod(name);
    }
}
