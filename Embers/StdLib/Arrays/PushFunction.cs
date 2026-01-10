using Embers.Language;
using Embers.Exceptions;

namespace Embers.StdLib.Arrays
{
    [StdLib("push")]
    public class PushFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count < 2 || values[0] == null)
                throw new ArgumentError("push expects an array and a value");

            if (values[0] is IList<object> arr)
            {
                arr.Add(values[1]);
                return arr;
            }

            throw new TypeError("push expects an array as first argument");
        }
    }
}

