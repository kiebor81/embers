using Embers.Annotations;
using Embers.Utilities;
using System.Collections;

namespace Embers.StdLib.Objects;

[StdLib("frozen?", TargetTypes = new[] { "Array", "Hash" })]
public class FrozenFunction : StdFunction
{
    [Comments("Returns true if the receiver is frozen.")]
    [Returns(ReturnType = typeof(bool))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0)
            return self.IsFrozen;

        var target = values[0];

        if (target is DynamicArray array)
            return array.IsFrozen;

        if (target is DynamicHash hash)
            return hash.IsFrozen;

        if (target is DynamicObject dynObject)
            return dynObject.IsFrozen;

        if (target is IList || target is IDictionary)
            return FrozenState.IsFrozen(target);

        return false;
    }
}
