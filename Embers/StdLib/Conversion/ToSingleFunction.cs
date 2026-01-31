using Embers.Annotations;
using Embers.Exceptions;

namespace Embers.StdLib.Conversion;

/// <summary>
/// Converts the value to a single-precision floating point number.
/// </summary>
[StdLib("to_single", "to_f32", TargetTypes = new[] { "Fixnum", "Float", "String", "NilClass", "TrueClass", "FalseClass", "Array", "Hash", "Range", "DateTime" })]
public class ToSingleFunction : StdFunction
{
    [Comments("Converts the value to a single-precision floating point number.")]
    [Arguments(ParamNames = new[] { "value" }, ParamTypes = new[] { typeof(object) })]
    [Returns(ReturnType = typeof(Number))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0 || values[0] == null)
            return 0f;

        var value = values[0];
        try
        {
            if (value is float f) return f;
            if (value is double d) return (float)d;
            if (value is long l) return (float)l;
            if (value is int i) return (float)i;
            if (value is string s) return float.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
            return Convert.ToSingle(value);
        }
        catch
        {
            throw new TypeError("to_single expects a numeric or string value");
        }
    }
}
