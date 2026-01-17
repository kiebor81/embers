using Embers.Annotations;
using Embers.StdLib.Numeric;
using Embers.Exceptions;

namespace Embers.StdLib.Ranges;

[StdLib("step", TargetType = "Range")]
public class StepFunction : StdFunction
{
    [Comments("Iterates over the range, yielding every `step`-th element to the given block.")]
    [Arguments(ParamNames = new[] { "range", "step" }, ParamTypes = new[] { typeof(Language.Primitive.Range), typeof(Number) })]
    [Returns(ReturnType = typeof(Language.Primitive.Range))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values.Count != 2)
            throw new ArgumentError($"wrong number of arguments (given {values.Count - 1}, expected 1)");

        var range = values[0] as Language.Primitive.Range;
        if (range == null)
            throw new TypeError("range must be a Range");

        if (!NumericCoercion.TryGetDouble(values[1], out var stepValue))
            throw new TypeError("step must be numeric");

        if (stepValue <= 0)
            throw new ArgumentError("step must be positive");

        if (context.Block == null)
            throw new ArgumentError("no block given");

        if (range.TryGetLongBounds(out var longStart, out var longEnd)
            && NumericCoercion.TryGetLong(values[1], out var longStep))
        {
            if (longStep <= 0)
                throw new ArgumentError("step must be positive");
            if (longStart <= longEnd)
            {
                for (var value = longStart; value <= longEnd; value += longStep)
                    context.Block.Apply(self, context, [value]);
            }

            return values[0];
        }

        if (range.TryGetDoubleBounds(out var doubleStart, out var doubleEnd))
        {
            if (doubleStart <= doubleEnd)
            {
                for (var value = doubleStart; value <= doubleEnd + 1e-9; value += stepValue)
                    context.Block.Apply(self, context, [value]);
            }

            return values[0];
        }

        throw new TypeError("range must be numeric");
    }
}
