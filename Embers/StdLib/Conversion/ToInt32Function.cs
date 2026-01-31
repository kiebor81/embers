using Embers.Annotations;
using Embers.Exceptions;

namespace Embers.StdLib.Conversion;

/// <summary>
/// Converts the value to a 32-bit integer.
/// </summary>
[StdLib("to_int32", "to_i32", TargetTypes = new[] { "Fixnum", "Float", "String", "NilClass", "TrueClass", "FalseClass", "Array", "Hash", "Range", "DateTime" })]
public class ToInt32Function : StdFunction
{
    [Comments("Converts the value to a 32-bit integer.")]
    [Arguments(ParamNames = new[] { "value" }, ParamTypes = new[] { typeof(object) })]
    [Returns(ReturnType = typeof(Number))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0 || values[0] == null)
            return 0;

        var value = values[0];
        try
        {
            if (value is int i) return i;
            if (value is long l) return (int)l;
            if (value is double d) return (int)d;
            if (value is float f) return (int)f;
            if (value is string s) return int.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
            if (value is bool b) return b ? 1 : 0;
            return Convert.ToInt32(value);
        }
        catch
        {
            throw new TypeError("to_int32 expects a numeric or string value");
        }
    }
}
