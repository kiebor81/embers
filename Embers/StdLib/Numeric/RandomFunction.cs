using Embers.Language;
using Embers.Exceptions;
using Embers.Annotations;

namespace Embers.StdLib.Numeric;

[StdLib("random", TargetTypes = new[] { "Fixnum", "Float" })]
public class RandomFunction : StdFunction
{
    private static readonly Random _random = new();

    [Comments("Generates a random number. If max is provided, returns an integer in [0, max). Otherwise, returns a float in [0, 1).")]
    [Arguments(ParamNames = new[] { "max" }, ParamTypes = new[] { typeof(Number) })]
    [Returns(ReturnType = typeof(Number))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        // If no argument, return double in [0,1)
        if (values == null || values.Count == 0 || values[0] == null)
            return _random.NextDouble();

        var value = values[0];
        if (!NumericCoercion.TryGetDouble(value, out var d))
            throw new TypeError("random expects a numeric argument");

        int max = (int)d;

        if (max <= 0)
            throw new ArgumentError("random expects a positive number");

        return _random.Next(max);
    }
}
