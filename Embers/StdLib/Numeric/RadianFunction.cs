using Embers.Language;
using Embers.Exceptions;

namespace Embers.StdLib.Numeric
{
    [StdLib("radian", TargetTypes = new[] { "Fixnum", "Float" })]
    public class RadianFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                throw new ArgumentError("radian expects a numeric argument");

            double degrees;
            if (values[0] is long l) degrees = l;
            else if (values[0] is int i) degrees = i;
            else if (values[0] is double d) degrees = d;
            else throw new TypeError("radian expects a numeric argument");

            return degrees * Math.PI / 180.0;
        }
    }
}

