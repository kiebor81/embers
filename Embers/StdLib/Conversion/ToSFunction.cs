using Embers.Language;
using Embers.Annotations;

namespace Embers.StdLib.Conversion;

/// <summary>
/// Converts the value to a string.
/// </summary>
[StdLib("to_s", "to_string", TargetTypes = new[] { "Fixnum", "Float", "String", "NilClass", "TrueClass", "FalseClass", "Array", "Hash", "Range", "DateTime" })]
public class ToSFunction : StdFunction
{
    [Comments("Converts the value to a string.")]
    [Arguments(ParamNames = new[] { "value" }, ParamTypes = new[] { typeof(object) })]
    [Returns(ReturnType = typeof(string))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0)
            return "";

        var value = values[0];
        return value?.ToString() ?? "";
    }
}
