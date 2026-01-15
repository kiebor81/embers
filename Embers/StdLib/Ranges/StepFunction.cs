using Embers.Language;
using Embers.Annotations;

namespace Embers.StdLib.Ranges;

[StdLib("step", TargetType = "Range")]
public class StepFunction : StdFunction
{
    [Comments("Iterates over the range, yielding every `step`-th element to the given block.")]
    [Arguments(ParamNames = new[] { "range", "step" }, ParamTypes = new[] { typeof(Language.Range), typeof(Number) })]
    [Returns(ReturnType = typeof(Language.Range))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values.Count != 2)
            throw new Exceptions.ArgumentError($"wrong number of arguments (given {values.Count - 1}, expected 1)");

        var range = values[0] as IEnumerable<int>;
        if (range == null)
            throw new Exceptions.TypeError("range must be a Range");

        if (values[1] is not int and not long)
            throw new Exceptions.TypeError("step must be an integer");

        int step = Convert.ToInt32(values[1]);

        if (step <= 0)
            throw new Exceptions.ArgumentError("step must be positive");

        if (context.Block == null)
            throw new Exceptions.ArgumentError("no block given");

        int count = 0;
        foreach (var value in range)
        {
            if (count % step == 0)
                context.Block.Apply(self, context, [value]);
            count++;
        }

        return values[0];
    }
}
