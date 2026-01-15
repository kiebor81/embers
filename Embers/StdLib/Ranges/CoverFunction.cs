using Embers.Language;
using Embers.Annotations;

namespace Embers.StdLib.Ranges;

[StdLib("cover?", TargetType = "Range")]
public class CoverFunction : StdFunction
{
    [Comments("Returns true if the range covers the given value.")]
    [Arguments(ParamNames = new[] { "range", "value" }, ParamTypes = new[] { typeof(Language.Range), typeof(object) })]
    [Returns(ReturnType = typeof(Boolean))]
    public override object Apply(DynamicObject self, Context context, IList<object> values) =>
        // cover? is an alias for include? in ranges
        new IncludeFunction().Apply(self, context, values);
}
