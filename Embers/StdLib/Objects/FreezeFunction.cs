using Embers.Annotations;
using Embers.Utilities;
using System.Collections;

namespace Embers.StdLib.Objects;

[StdLib("freeze", TargetTypes = new[] { "Array", "Hash" })]
public class FreezeFunction : StdFunction
{
    [Comments("Freezes the receiver to prevent in-place mutation.")]
    [Returns(ReturnType = typeof(object))]
    public override object? Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0)
        {
            self.Freeze();
            return self;
        }

        var target = values[0];

        if (target is DynamicArray array)
            array.Freeze();
        else if (target is DynamicHash hash)
            hash.Freeze();
        else if (target is DynamicObject dynObject)
            dynObject.Freeze();
        else if (target is IList || target is IDictionary)
            FrozenState.Freeze(target);

        return target;
    }
}
