using Embers.Language;
using Embers.Annotations;

namespace Embers.StdLib.Ranges;

[StdLib("count", TargetType = "Range")]
public class CountFunction : StdFunction
{
    [Comments("Returns the number of elements in the range.")]
    [Arguments(ParamNames = new[] { "range" }, ParamTypes = new[] { typeof(object) })]
    [Returns(ReturnType = typeof(Number))]
    public override object Apply(DynamicObject self, Context context, IList<object> values) =>
        // count is an alias for size in ranges
        new SizeFunction().Apply(self, context, values);
}
