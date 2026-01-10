using Embers.Language;
using Embers.Exceptions;

namespace Embers.StdLib.Strings
{
    [StdLib("include?", TargetType = "String")]
    public class IncludeFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count < 2 || values[0] == null || values[1] == null)
                return false;

            var collection = values[0];
            var item = values[1];

            if (collection is string s)
                return s.Contains(item.ToString());
            if (collection is IEnumerable<object> arr)
                return arr.Contains(item);

            throw new ArgumentError("include? expects a string or array as first argument");
        }
    }
}

