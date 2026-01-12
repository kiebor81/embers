using Embers.Language;
using Embers.Exceptions;

namespace Embers.StdLib.Numeric
{
    [StdLib("random", TargetTypes = new[] { "Fixnum", "Float" })]
    public class RandomFunction : StdFunction
    {
        private static readonly Random _random = new();

        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            // If no argument, return double in [0,1)
            if (values == null || values.Count == 0 || values[0] == null)
                return _random.NextDouble();

            var value = values[0];
            int max;
            if (value is long l) max = (int)l;
            else if (value is int i) max = i;
            else if (value is double d) max = (int)d;
            else throw new TypeError("random expects a numeric argument");

            if (max <= 0)
                throw new ArgumentError("random expects a positive number");

            return _random.Next(max);
        }
    }
}
