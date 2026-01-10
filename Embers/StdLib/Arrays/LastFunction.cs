using Embers.Language;
using Embers.Exceptions;

namespace Embers.StdLib.Arrays
{
    [StdLib("last")]
    public class LastFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                throw new ArgumentError("last expects an array argument");

            if (values[0] is IList<object> arr)
            {
                if (arr.Count == 0)
                    return null;
                return arr[arr.Count - 1];
            }

            throw new TypeError("last expects an array");
        }
    }
}

