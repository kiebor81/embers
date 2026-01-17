using Embers.Functions;

namespace Embers.Language.Native;

/// <summary>
/// DynamicClass wrapper for native types to allow reopening and method lookup.
/// </summary>
public sealed class NativeClassAdapter(DynamicClass @class, string name, DynamicClass superclass, DynamicClass parent, NativeClass nativeClass) : DynamicClass(@class, name, superclass, parent)
{
    public NativeClass NativeClass { get; } = nativeClass ?? throw new ArgumentNullException(nameof(nativeClass));

    public override IFunction? GetMethod(string name)
    {
        var method = base.GetMethod(name);
        if (method != null)
            return method;

        return NativeClass.GetMethod(name);
    }
}
