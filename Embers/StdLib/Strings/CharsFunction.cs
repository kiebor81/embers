using Embers.Language;
using Embers.Exceptions;

namespace Embers.StdLib.Strings
{
    [StdLib("chars", TargetType = "String")]
    public class CharsFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                throw new ArgumentError("chars expects a string argument");

            if (values[0] is string s)
                return s.Select(c => c.ToString()).ToList();

            throw new TypeError("chars expects a string");
        }
    }
}

