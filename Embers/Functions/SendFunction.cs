using Embers.Annotations;

namespace Embers.Functions;

/// <summary>
/// Represents the 'send' function which sends a message to an object.
/// </summary>
[ScannerIgnore]
public sealed class SendFunction : ICallableWithBlock
{
    public object Apply(DynamicObject self, Context context, IList<object> values)
        => Machine.Send(self, context, values, null);

    public object ApplyWithBlock(DynamicObject self, Context context, IList<object> values, IFunction? block)
        => Machine.Send(self, context, values, block);
}
