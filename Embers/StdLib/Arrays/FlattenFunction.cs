using Embers.Language;
using Embers.Exceptions;

namespace Embers.StdLib.Arrays
{
    [StdLib("flatten")]
    public class FlattenFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                throw new ArgumentError("flatten expects an array argument");

            if (values[0] is IEnumerable<object> arr)
            {
                var result = new List<object>();
                foreach (var item in arr)
                {
                    if (item is IEnumerable<object> subarr)
                        result.AddRange(subarr);
                    else
                        result.Add(item);
                }
                return result;
            }

            throw new TypeError("flatten expects an array");
        }
    }
}

