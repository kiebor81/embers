using Embers.Annotations;

namespace Embers.Functions;

/// <summary>
/// Function that defines a method on an object.
/// </summary>
[ScannerIgnore]
public sealed class DefineMethodFunction : ICallableWithBlock
{
    public object Apply(DynamicObject self, Context context, IList<object> values)
        => Machine.DefineMethod(self, context, values, null);

    public object ApplyWithBlock(DynamicObject self, Context context, IList<object> values, IFunction? block)
        => Machine.DefineMethod(self, context, values, block);
}
