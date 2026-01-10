using Embers.Language;
using Embers.Exceptions;
using System.Collections;

namespace Embers.StdLib.Arrays
{
    [StdLib("compact", TargetType = "Array")]
    public class CompactFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                throw new ArgumentError("compact expects an array argument");

            if (values[0] is IEnumerable arr)
            {
                var result = new DynamicArray();
                foreach (var item in arr)
                    if (item != null)
                        result.Add(item);
                return result;
            }

            throw new TypeError("compact expects an array");
        }
    }
}

