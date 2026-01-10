using Embers.Language;
using Embers.Exceptions;

namespace Embers.StdLib.Arrays
{
    [StdLib("shift")]
    public class ShiftFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                throw new ArgumentError("shift expects an array argument");

            if (values[0] is IList<object> arr)
            {
                if (arr.Count == 0)
                    return null;
                var first = arr[0];
                arr.RemoveAt(0);
                return first;
            }

            throw new TypeError("shift expects an array");
        }
    }
}

