using Embers.Language;
using Embers.Exceptions;
using Embers.Annotations;

namespace Embers.StdLib.Numeric
{
    [StdLib("tan", TargetTypes = new[] { "Fixnum", "Float" })]
    public class TanFunction : StdFunction
    {
        [Comments("Returns the tangent of a number (in radians).")]
        [Arguments(ParamNames = new[] { "number" }, ParamTypes = new[] { typeof(Number) })]
        [Returns(ReturnType = typeof(Number))]
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                throw new ArgumentError("tan expects a numeric argument");

            var value = values[0];
            double d;
            if (value is long l) d = l;
            else if (value is int i) d = i;
            else if (value is double dd) d = dd;
            else throw new TypeError("tan expects a numeric argument");

            return Math.Tan(d);
        }
    }
}
