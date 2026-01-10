using Embers.Language;
using Embers.Exceptions;

namespace Embers.StdLib.Numeric
{
    [StdLib("random_seed", TargetTypes = new[] { "Fixnum", "Float" })]
    public class RandomSeedFunction : StdFunction
    {
        // Shared random instance for all random functions
        private static Random _random = new();

        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                throw new ArgumentError("random_seed expects a numeric argument");

            int seed;
            if (values[0] is int i) seed = i;
            else if (values[0] is double d) seed = (int)d;
            else throw new TypeError("random_seed expects a numeric argument");

            _random = new Random(seed);
            return null;
        }

        // Expose the shared random instance for other functions
        public static Random SharedRandom => _random;
    }
}

