using Embers.Annotations;
using Embers.Exceptions;

namespace Embers.StdLib.Conversion;

/// <summary>
/// Converts the value to a decimal number.
/// </summary>
[StdLib("to_decimal", TargetTypes = new[] { "Fixnum", "Float", "String", "NilClass", "TrueClass", "FalseClass", "Array", "Hash", "Range", "DateTime" })]
public class ToDecimalFunction : StdFunction
{
    [Comments("Converts the value to a decimal number.")]
    [Arguments(ParamNames = new[] { "value" }, ParamTypes = new[] { typeof(object) })]
    [Returns(ReturnType = typeof(Number))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0 || values[0] == null)
            return 0m;

        var value = values[0];
        try
        {
            if (value is decimal m) return m;
            if (value is double d) return (decimal)d;
            if (value is float f) return (decimal)f;
            if (value is long l) return (decimal)l;
            if (value is int i) return (decimal)i;
            if (value is string s) return decimal.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
            return Convert.ToDecimal(value);
        }
        catch
        {
            throw new TypeError("to_decimal expects a numeric or string value");
        }
    }
}
