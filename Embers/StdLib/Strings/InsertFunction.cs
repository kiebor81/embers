using Embers.Language;
using Embers.Exceptions;

namespace Embers.StdLib.Strings
{
    [StdLib("insert", TargetType = "String")]
    public class InsertFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count < 3 || values[0] == null || values[1] == null || values[2] == null)
                throw new ArgumentError("insert expects a string, index, and substring");

            if (values[0] is string s && values[1] is int index && values[2] is string sub)
            {
                if (index < 0 || index > s.Length)
                    throw new ArgumentError("insert: index out of bounds");
                return s.Substring(0, index) + sub + s.Substring(index);
            }

            throw new TypeError("insert expects a string, integer, and string");
        }
    }
}

