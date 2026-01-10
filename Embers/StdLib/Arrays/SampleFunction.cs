using Embers.Language;
using Embers.Exceptions;
using System.Collections;

namespace Embers.StdLib.Arrays
{
    [StdLib("sample", TargetType = "Array")]
    public class SampleFunction : StdFunction
    {
        private static readonly Random _random = new();

        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                throw new ArgumentError("sample expects an array argument");

            if (values[0] is IList arr)
            {
                if (arr.Count == 0)
                    return null;
                return arr[_random.Next(arr.Count)];
            }

            throw new TypeError("sample expects an array");
        }
    }
}

