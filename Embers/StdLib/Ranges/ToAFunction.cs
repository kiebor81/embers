using Embers.Annotations;

namespace Embers.StdLib.Ranges;

[StdLib("to_a", TargetType = "Range")]
public class ToAFunction : StdFunction
{
    [Comments("Converts the range to an array.")]
    [Arguments(ParamNames = new[] { "range" }, ParamTypes = new[] { typeof(Language.Primitive.Range) })]
    [Returns(ReturnType = typeof(Array))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values.Count != 1)
            throw new Exceptions.ArgumentError($"wrong number of arguments (given {values.Count - 1}, expected 0)");

        var range = values[0] as Language.Primitive.Range;
        if (range == null)
            throw new Exceptions.TypeError("range must be a Range");

        var result = new DynamicArray();
        foreach (var value in range.Enumerate())
            result.Add(value);

        return result;
    }
}
