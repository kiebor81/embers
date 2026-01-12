using Embers.Language;
using Embers.Exceptions;

namespace Embers.StdLib.Numeric
{
    [StdLib("sign", TargetTypes = new[] { "Fixnum", "Float" })]
    public class SignFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                throw new ArgumentError("sign expects a numeric argument");

            var value = values[0];
            double d;
            if (value is long l) d = l;
            else if (value is int i) d = i;
            else if (value is double dd) d = dd;
            else throw new TypeError("sign expects a numeric argument");

            return Math.Sign(d);
        }
    }
}
