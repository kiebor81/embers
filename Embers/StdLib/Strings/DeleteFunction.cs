using Embers.Language;
using Embers.Exceptions;

namespace Embers.StdLib.Strings
{
    [StdLib("delete", TargetType = "String")]
    public class DeleteFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count < 2 || values[0] == null || values[1] == null)
                throw new ArgumentError("delete expects a string and a substring");

            if (values[0] is string s && values[1] is string sub)
                return s.Replace(sub, "");

            throw new TypeError("delete expects string arguments");
        }
    }
}

