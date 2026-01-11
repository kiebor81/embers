using Embers.Language;

namespace Embers.StdLib.Ranges
{
    [StdLib("map", TargetType = "Range")]
    public class MapFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values.Count != 1)
                throw new Exceptions.ArgumentError($"wrong number of arguments (given {values.Count - 1}, expected 0)");

            var range = values[0] as IEnumerable<int>;
            if (range == null)
                throw new Exceptions.TypeError("range must be a Range");

            if (context.Block == null)
                throw new Exceptions.ArgumentError("no block given");

            var result = new DynamicArray();
            foreach (var value in range)
            {
                var blockResult = context.Block.Apply(self, context, [value]);
                result.Add(blockResult);
            }

            return result;
        }
    }
}
