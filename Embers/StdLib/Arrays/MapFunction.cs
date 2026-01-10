using Embers.Language;
using Embers.Exceptions;
using Embers.Functions;

namespace Embers.StdLib.Arrays
{
    [StdLib("map", TargetType = "Array")]
    public class MapFunction : StdFunction
    {
        public override object ApplyWithBlock(DynamicObject self, Context context, IList<object> values, IFunction block)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                throw new ArgumentError("map expects an array argument");

            if (block == null)
                throw new ArgumentError("map expects a block");

            if (values[0] is IEnumerable<object> arr)
            {
                var result = new List<object>();
                var blockContext = new Context(self, context, block); // this injects the block cleanly

                foreach (var item in arr)
                    result.Add(block.Apply(self, blockContext, [item]));

                return result;
            }

            throw new TypeError("map expects an array");
        }

        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            return ApplyWithBlock(self, context, values, null); // fallback path
        }

        //public override object Apply(DynamicObject self, Context context, IList<object> values)
        //{
        //    if (values == null || values.Count == 0 || values[0] == null)
        //        throw new ArgumentError("map expects an array argument");

        //    if (context.Block == null)
        //        throw new ArgumentError("map expects a block");

        //    if (values[0] is IEnumerable<object> arr)
        //    {
        //        var result = new List<object>();
        //        foreach (var item in arr)
        //            result.Add(context.Block.Apply(self, context, [item]));
        //        return result;
        //    }

        //    throw new TypeError("map expects an array");
        //}
    }
}
