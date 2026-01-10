using Embers.Language;
using Embers.Exceptions;
using Embers.Functions;
using System.Collections;

namespace Embers.StdLib.Arrays
{
    [StdLib("map", TargetType = "Array")]
    public class MapFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                throw new ArgumentError("map expects an array argument");

            if (context.Block == null)
                throw new ArgumentError("map expects a block");

            if (values[0] is IEnumerable arr)
            {
                var result = new DynamicArray();
                foreach (var item in arr)
                    result.Add(context.Block.Apply(self, context, [item]));
                return result;
            }

            throw new TypeError("map expects an array");
        }
    }
}
