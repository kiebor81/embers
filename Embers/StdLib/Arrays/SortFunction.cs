using Embers.Language;
using Embers.Exceptions;

namespace Embers.StdLib.Arrays
{
    /// <summary>
    /// Returns a sorted copy of the array.
    /// </summary>
    [StdLib("sort")]
    public class SortFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                throw new ArgumentError("sort expects an array argument");

            if (values[0] is IEnumerable<object> arr)
                return arr.OrderBy(x => x).ToList();

            throw new TypeError("sort expects an array");
        }
    }
}

