using Embers.Language;
using Embers.Exceptions;

namespace Embers.StdLib.Strings
{
    [StdLib("start_with?", TargetType = "String")]
    public class StartWithFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count < 2 || values[0] == null || values[1] == null)
                throw new ArgumentError("start_with? expects a string and a prefix");

            if (values[0] is string s && values[1] is string prefix)
                return s.StartsWith(prefix);

            throw new TypeError("start_with? expects string arguments");
        }
    }
}