using Embers.Exceptions;
using Embers.Annotations;

namespace Embers.StdLib.Conversion;

/// <summary>
/// Converts the value to an integer.
/// </summary>
[StdLib("to_i", "to_int", "to_integer", TargetTypes = new[] { "Fixnum", "Float", "String", "NilClass", "TrueClass", "FalseClass", "Array", "Hash", "Range", "DateTime" })]
public class ToIFunction : StdFunction
{
    [Comments("Converts the value to an integer.")]
    [Arguments(ParamNames = new[] { "value" }, ParamTypes = new[] { typeof(object) })]
    [Returns(ReturnType = typeof(Number))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0 || values[0] == null)
            return 0L;

        var value = values[0];
        try
        {
            if (value is long l) return l;
            if (value is int i) return (long)i;
            if (value is double d) return (long)d;
            if (value is string s) return long.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
            if (value is bool b) return b ? 1L : 0L;
            return Convert.ToInt64(value);
        }
        catch
        {
            throw new TypeError("to_i expects a numeric or string value");
        }
    }
}
