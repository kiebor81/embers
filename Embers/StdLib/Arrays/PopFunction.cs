using Embers.Language;
using Embers.Exceptions;

namespace Embers.StdLib.Arrays
{
    [StdLib("pop")]
    public class PopFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                throw new ArgumentError("pop expects an array argument");

            if (values[0] is IList<object> arr)
            {
                if (arr.Count == 0)
                    return null;
                var last = arr[arr.Count - 1];
                arr.RemoveAt(arr.Count - 1);
                return last;
            }

            throw new TypeError("pop expects an array");
        }
    }
}

