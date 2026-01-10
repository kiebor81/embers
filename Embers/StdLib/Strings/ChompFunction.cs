using Embers.Language;
using Embers.Exceptions;

namespace Embers.StdLib.Strings
{
    /// <summary>
    /// Removes trailing newline characters from a string.
    /// </summary>
    [StdLib("chomp", TargetType = "String")]
    public class ChompFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                throw new ArgumentError("chomp expects a string argument");

            if (values[0] is string s)
                return s.TrimEnd('\r', '\n');

            throw new TypeError("chomp expects a string");
        }
    }
}

