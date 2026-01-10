using Embers.Language;
using Embers.Exceptions;

namespace Embers.StdLib.Arrays
{
    [StdLib("compact")]
    public class CompactFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                throw new ArgumentError("compact expects an array argument");

            if (values[0] is IEnumerable<object> arr)
                return arr.Where(x => x != null).ToList();

            throw new TypeError("compact expects an array");
        }
    }
}

