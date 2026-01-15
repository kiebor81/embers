using Embers.Language;
using Embers.Annotations;

namespace Embers.StdLib.Conversion;

/// <summary>
/// Returns the inspect-style string representation of the value.
/// </summary>
[StdLib("inspect", TargetTypes = new[] { "Fixnum", "Float", "String", "NilClass", "TrueClass", "FalseClass", "Array", "Hash", "Range", "DateTime" })]
public class InspectFunction : StdFunction
{
    [Comments("Returns the inspect-style string representation of the value.")]
    [Arguments(ParamNames = new[] { "value" }, ParamTypes = new[] { typeof(object) })]
    [Returns(ReturnType = typeof(string))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0)
            return InspectUtils.Inspect(self);

        return InspectUtils.Inspect(values[0]);
    }
}
