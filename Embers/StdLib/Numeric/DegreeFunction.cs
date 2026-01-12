using Embers.Language;
using Embers.Exceptions;
using Embers.Annotations;

namespace Embers.StdLib.Numeric
{
    [StdLib("degree", TargetTypes = new[] { "Fixnum", "Float" })]
    public class DegreeFunction : StdFunction
    {
        [Comments("Converts radians to degrees.")]
        [Arguments(ParamNames = new[] { "radians" }, ParamTypes = new[] { typeof(Number) })]
        [Returns(ReturnType = typeof(Number))]
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                throw new ArgumentError("degree expects a numeric argument");

            double radians;
            if (values[0] is long l) radians = l;
            else if (values[0] is int i) radians = i;
            else if (values[0] is double d) radians = d;
            else throw new TypeError("degree expects a numeric argument");

            return radians * 180.0 / Math.PI;
        }
    }
}

