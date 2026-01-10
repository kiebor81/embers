using Embers.Language;
using Embers.Exceptions;

namespace Embers.StdLib.Arrays
{
    /// <summary>
    /// Joins array elements into a string, separated by the given separator.
    /// </summary>
    [StdLib("join")]
    public class JoinFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                throw new ArgumentError("join expects an array argument");

            var separator = values.Count > 1 ? values[1]?.ToString() ?? "" : "";

            if (values[0] is IEnumerable<object> arr)
                return string.Join(separator, arr.Select(x => x?.ToString()));

            throw new TypeError("join expects an array");
        }
    }
}

