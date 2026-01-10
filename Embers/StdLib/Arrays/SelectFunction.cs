using Embers.Language;
using Embers.Exceptions;

namespace Embers.StdLib.Arrays
{
    [StdLib("select", TargetType = "Array")]
    public class SelectFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                throw new ArgumentError("select expects an array argument");

            if (context.Block == null)
                throw new ArgumentError("select expects a block");

            if (values[0] is IEnumerable<object> arr)
            {
                var result = new List<object>();
                foreach (var item in arr)
                {
                    var keep = context.Block.Apply(self, context, [item]);
                    if (keep is bool b && b)
                        result.Add(item);
                }
                return result;
            }

            throw new TypeError("select expects an array");
        }
    }
}
