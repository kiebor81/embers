using Embers.Language;
using Embers.Exceptions;

namespace Embers.StdLib.Strings
{
    [StdLib("replace", TargetType = "String")]
    public class ReplaceFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count < 2 || values[0] == null || values[1] == null)
                throw new ArgumentError("replace expects two string arguments");

            if (values[0] is string s && values[1] is string replacement)
                return replacement;

            throw new TypeError("replace expects string arguments");
        }
    }
}

