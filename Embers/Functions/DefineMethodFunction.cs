namespace Embers.Functions;

public sealed class DefineMethodFunction : ICallableWithBlock
{
    public object Apply(DynamicObject self, Context context, IList<object> values)
        => Machine.DefineMethod(self, context, values, null);

    public object ApplyWithBlock(DynamicObject self, Context context, IList<object> values, IFunction? block)
        => Machine.DefineMethod(self, context, values, block);
}
