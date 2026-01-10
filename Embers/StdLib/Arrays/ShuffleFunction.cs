using Embers.Language;
using Embers.Exceptions;

namespace Embers.StdLib.Arrays
{
    [StdLib("shuffle")]
    public class ShuffleFunction : StdFunction
    {
        private static readonly Random _random = new();

        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                throw new ArgumentError("shuffle expects an array argument");

            if (values[0] is IList<object> arr)
            {
                var copy = new List<object>(arr);
                int n = copy.Count;
                while (n > 1)
                {
                    n--;
                    int k = _random.Next(n + 1);
                    (copy[n], copy[k]) = (copy[k], copy[n]);
                }
                return copy;
            }

            throw new TypeError("shuffle expects an array");
        }
    }
}

