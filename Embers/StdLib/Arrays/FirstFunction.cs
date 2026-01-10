using Embers.Language;
using Embers.Exceptions;

namespace Embers.StdLib.Arrays
{
    [StdLib("first")]
    public class FirstFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                throw new ArgumentError("first expects an array argument");

            if (values[0] is IList<object> arr)
            {
                if (arr.Count == 0)
                    return null;
                return arr[0];
            }

            throw new TypeError("first expects an array");
        }
    }
}

