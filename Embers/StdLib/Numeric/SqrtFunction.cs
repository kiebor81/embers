using Embers.Language;
using Embers.Exceptions;

namespace Embers.StdLib.Numeric
{
    [StdLib("sqrt", TargetTypes = new[] { "Fixnum", "Float" })]
    public class SqrtFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                throw new ArgumentError("sqrt expects a numeric argument");

            var value = values[0];
            double d;
            if (value is int i) d = i;
            else if (value is double dd) d = dd;
            else throw new TypeError("sqrt expects a numeric argument");

            if (d < 0)
                throw new ArgumentError("sqrt expects a non-negative number");

            return Math.Sqrt(d);
        }
    }
}
