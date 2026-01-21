namespace Embers.Functions;

public sealed class SendFunction : ICallableWithBlock
{
    public object Apply(DynamicObject self, Context context, IList<object> values)
        => Machine.Send(self, context, values, null);

    public object ApplyWithBlock(DynamicObject self, Context context, IList<object> values, IFunction? block)
        => Machine.Send(self, context, values, block);
}
