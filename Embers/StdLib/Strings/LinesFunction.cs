using Embers.Language;
using Embers.Exceptions;

namespace Embers.StdLib.Strings
{
    [StdLib("lines", TargetType = "String")]
    public class LinesFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                throw new ArgumentError("lines expects a string argument");

            if (values[0] is string s)
                return s.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None).ToList();

            throw new TypeError("lines expects a string");
        }
    }
}

