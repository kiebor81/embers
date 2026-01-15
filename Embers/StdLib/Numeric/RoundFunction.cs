using Embers.Language;
using Embers.Exceptions;
using Embers.Annotations;

namespace Embers.StdLib.Numeric;

[StdLib("round", TargetTypes = new[] { "Fixnum", "Float" })]
public class RoundFunction : StdFunction
{
    [Comments("Rounds a number to the nearest integer.")]
    [Arguments(ParamNames = new[] { "number" }, ParamTypes = new[] { typeof(Number) })]
    [Returns(ReturnType = typeof(Number))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0 || values[0] == null)
            throw new ArgumentError("round expects a numeric argument");

        var value = values[0];
        double d;
        if (value is long l) d = l;
        else if (value is int i) d = i;
        else if (value is double dd) d = dd;
        else throw new TypeError("round expects a numeric argument");

        return (long)Math.Round(d);
    }
}
