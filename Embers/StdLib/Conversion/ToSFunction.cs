using Embers.Language;

namespace Embers.StdLib.Conversion
{
    /// <summary>
    /// Converts the value to a string.
    /// </summary>
    [StdLib("to_s", "to_string", TargetTypes = new[] { "Fixnum", "Float", "String", "NilClass", "TrueClass", "FalseClass", "Array", "Hash", "Range", "DateTime" })]
    public class ToSFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0)
                return "";

            var value = values[0];
            return value?.ToString() ?? "";
        }
    }
}
