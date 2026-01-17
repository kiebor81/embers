using Embers.Language;
using Embers.Annotations;
using Embers.Exceptions;

namespace Embers.StdLib.Ranges;

[StdLib("size", "count", TargetType = "Range")]
public class SizeFunction : StdFunction
{
    [Comments("Returns the number of elements in the range.")]
    [Arguments(ParamNames = new[] { "range" }, ParamTypes = new[] { typeof(Language.Range) })]
    [Returns(ReturnType = typeof(Number))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values.Count != 1)
            throw new ArgumentError($"wrong number of arguments (given {values.Count - 1}, expected 0)");

        if (values[0] is not IEnumerable<int> range)
            throw new TypeError("range must be a Range");

        int count = 0;
        foreach (var _ in range)
            count++;

        return count;
    }
}
