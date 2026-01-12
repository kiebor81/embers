using Embers.Language;
using Embers.Exceptions;

namespace Embers.StdLib.Numeric
{
    [StdLib("floor", TargetTypes = new[] { "Fixnum", "Float" })]
    public class FloorFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                throw new ArgumentError("floor expects a numeric argument");

            var value = values[0];
            double d;
            if (value is long l) d = l;
            else if (value is int i) d = i;
            else if (value is double dd) d = dd;
            else throw new TypeError("floor expects a numeric argument");

            return (long)Math.Floor(d);
        }
    }
}
