using Embers.Language;
using Embers.Exceptions;
using Embers.Annotations;

namespace Embers.StdLib.Numeric
{
    [StdLib("sign", TargetTypes = new[] { "Fixnum", "Float" })]
    public class SignFunction : StdFunction
    {
        [Comments("Returns the sign of a number: -1 for negative, 0 for zero, and 1 for positive.")]
        [Arguments(ParamNames = new[] { "number" }, ParamTypes = new[] { typeof(Number) })]
        [Returns(ReturnType = typeof(Number))]
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
