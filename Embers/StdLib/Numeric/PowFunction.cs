using Embers.Language;
using Embers.Exceptions;

namespace Embers.StdLib.Numeric
{
    [StdLib("pow", TargetTypes = new[] { "Fixnum", "Float" })]
    public class PowFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count < 2 || values[0] == null || values[1] == null)
                throw new ArgumentError("pow expects two numeric arguments");

            double x, y;
            if (values[0] is long l1) x = l1;
            else if (values[0] is int i1) x = i1;
            else if (values[0] is double d1) x = d1;
            else throw new TypeError("pow expects numeric arguments");

            if (values[1] is long l2) y = l2;
            else if (values[1] is int i2) y = i2;
            else if (values[1] is double d2) y = d2;
            else throw new TypeError("pow expects numeric arguments");

            return Math.Pow(x, y);
        }
    }
}
