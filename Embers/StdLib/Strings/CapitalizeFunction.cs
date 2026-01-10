using Embers.Language;
using Embers.Exceptions;

namespace Embers.StdLib.Strings
{
    /// <summary>
    /// Capitalizes the first character of a string.
    /// </summary>
    [StdLib("capitalize", TargetType = "String")]
    public class CapitalizeFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                return "";

            if (values[0] is string s && s.Length > 0)
                return char.ToUpper(s[0]) + s.Substring(1);

            throw new TypeError("capitalize expects a string");
        }
    }
}

