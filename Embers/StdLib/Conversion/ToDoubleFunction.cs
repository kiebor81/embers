using Embers.Annotations;
using Embers.Exceptions;

namespace Embers.StdLib.Conversion;

/// <summary>
/// Converts the value to a double-precision floating point number.
/// </summary>
[StdLib("to_double", "to_f64", TargetTypes = new[] { "Fixnum", "Float", "String", "NilClass", "TrueClass", "FalseClass", "Array", "Hash", "Range", "DateTime" })]
public class ToDoubleFunction : StdFunction
{
    [Comments("Converts the value to a double-precision floating point number.")]
    [Arguments(ParamNames = new[] { "value" }, ParamTypes = new[] { typeof(object) })]
    [Returns(ReturnType = typeof(Number))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0 || values[0] == null)
            return 0.0;

        var value = values[0];
        try
        {
            if (value is double d) return d;
            if (value is float f) return (double)f;
            if (value is long l) return (double)l;
            if (value is int i) return (double)i;
            if (value is string s) return double.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
            return Convert.ToDouble(value);
        }
        catch
        {
            throw new TypeError("to_double expects a numeric or string value");
        }
    }
}
