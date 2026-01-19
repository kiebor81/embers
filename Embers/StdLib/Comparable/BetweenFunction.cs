using Embers.Annotations;
using Embers.Exceptions;
using Embers.Utilities;

namespace Embers.StdLib.Comparable;

public sealed class BetweenFunction : StdFunction
{
    [Comments("Returns true if the value falls within the provided min/max bounds.")]
    [Arguments(ParamNames = new[] { "value", "min", "max" }, ParamTypes = new[] { typeof(object), typeof(object), typeof(object) })]
    [Returns(ReturnType = typeof(bool))]
    public override object? Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null)
            throw new ArgumentError("between? expects a value and two bounds");

        object? value;
        object? min;
        object? max;

        if (values.Count == 2)
        {
            value = self is NativeObject nativeObject ? nativeObject.NativeValue ?? self : self;
            min = values[0];
            max = values[1];
        }
        else if (values.Count == 3)
        {
            value = values[0];
            min = values[1];
            max = values[2];
        }
        else
        {
            throw new ArgumentError("between? expects a value and two bounds");
        }

        if (value == null)
            throw new ArgumentError("between? expects a value and two bounds");

        if (!ComparisonUtilities.TryCompare(value, min, out var minResult))
            return false;

        if (!ComparisonUtilities.TryCompare(value, max, out var maxResult))
            return false;

        return minResult >= 0 && maxResult <= 0;
    }
}
