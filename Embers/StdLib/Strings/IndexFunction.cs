using Embers.Language;
using Embers.Exceptions;

namespace Embers.StdLib.Strings
{
    /// <summary>
    /// Returns the index of a substring in a string or element in an array, or -1 if not found.
    /// </summary>
    [StdLib("index", TargetType = "String")]
    public class IndexFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count < 2 || values[0] == null || values[1] == null)
                throw new ArgumentError("index expects a collection and a value");

            var collection = values[0];
            var item = values[1];

            if (collection is string s)
                return s.IndexOf(item.ToString(), StringComparison.Ordinal);

            if (collection is IEnumerable<object> arr)
            {
                int i = 0;
                foreach (var v in arr)
                {
                    if (Equals(v, item)) return i;
                    i++;
                }
                return -1;
            }

            throw new TypeError("index expects a string or array");
        }
    }
}

