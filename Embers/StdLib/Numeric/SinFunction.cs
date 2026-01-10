using Embers.Language;
using Embers.Exceptions;

namespace Embers.StdLib.Numeric
{
    [StdLib("sin", TargetTypes = new[] { "Fixnum", "Float" })]
    public class SinFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                throw new ArgumentError("sin expects a numeric argument");

            var value = values[0];
            double d;
            if (value is int i) d = i;
            else if (value is double dd) d = dd;
            else throw new TypeError("sin expects a numeric argument");

            return Math.Sin(d);
        }
    }
}
