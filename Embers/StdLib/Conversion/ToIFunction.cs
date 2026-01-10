using Embers.Language;
using Embers.Exceptions;

namespace Embers.StdLib.Conversion
{
    /// <summary>
    /// Converts the value to an integer.
    /// </summary>
    [StdLib("to_i", "to_int", "to_integer", TargetTypes = new[] { "Fixnum", "Float", "String", "NilClass", "TrueClass", "FalseClass", "Array", "Hash", "Range", "DateTime" })]
    public class ToIFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                return 0;

            var value = values[0];
            try
            {
                if (value is int i) return i;
                if (value is double d) return (int)d;
                if (value is string s) return int.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
                return Convert.ToInt32(value);
            }
            catch
            {
                throw new TypeError("to_i expects a numeric or string value");
            }
        }
    }
}
