using Embers.Language;

namespace Embers.StdLib.Ranges
{
    [StdLib("max", TargetType = "Range")]
    public class MaxFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values.Count != 1)
                throw new Exceptions.ArgumentError($"wrong number of arguments (given {values.Count - 1}, expected 0)");

            var range = values[0] as IEnumerable<int>;
            if (range == null)
                throw new Exceptions.TypeError("range must be a Range");

            int? maxValue = null;
            foreach (var value in range)
            {
                if (maxValue == null || value > maxValue)
                    maxValue = value;
            }

            return maxValue;
        }
    }
}
