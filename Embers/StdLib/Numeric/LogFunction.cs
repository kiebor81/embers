using Embers.Language;
using Embers.Exceptions;

namespace Embers.StdLib.Numeric
{
    [StdLib("log", TargetTypes = new[] { "Fixnum", "Float" })]
    public class LogFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                throw new ArgumentError("log expects a numeric argument");

            var value = values[0];
            double d;
            if (value is int i) d = i;
            else if (value is double dd) d = dd;
            else throw new TypeError("log expects a numeric argument");

            if (d <= 0)
                throw new ArgumentError("log expects a positive number");

            return Math.Log(d);
        }
    }
}
