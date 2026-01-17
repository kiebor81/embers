namespace Embers.Functions;

/// <summary>
/// Adapts NativeClass instance methods to IFunction dispatch.
/// </summary>
public sealed class NativeStdFunction(NativeClass nativeClass, string name) : ICallableWithBlock
{
    private readonly NativeClass nativeClass = nativeClass;
    private readonly string name = name;

    public object Apply(DynamicObject self, Context context, IList<object> values) =>
        ApplyWithBlock(self, context, values, null);

    public object ApplyWithBlock(DynamicObject self, Context context, IList<object> values, IFunction? block)
    {
        if (self is not NativeObject nativeObject)
            return null;

        var callContext = block != null ? new Context(context.Self, context, block) : context;
        var method = nativeClass.GetInstanceMethod(name, callContext);
        return method != null ? method(nativeObject.NativeValue, values) : null;
    }
}
