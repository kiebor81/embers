using Embers.Language;
using Embers.Exceptions;

namespace Embers.StdLib.Strings
{
    [StdLib("end_with?", TargetType = "String")]
    public class EndWithFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count < 2 || values[0] == null || values[1] == null)
                throw new ArgumentError("end_with? expects a string and a suffix");

            if (values[0] is string s && values[1] is string suffix)
                return s.EndsWith(suffix);

            throw new TypeError("end_with? expects string arguments");
        }
    }
}

