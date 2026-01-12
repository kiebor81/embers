using Embers.Language;
using Embers.Exceptions;
using Embers.Annotations;

namespace Embers.StdLib.Numeric
{
    /// <summary>
    /// Returns the maximum value from a list of numbers.
    /// </summary>
    [StdLib("max", TargetTypes = new[] { "Fixnum", "Float" })]
    public class MaxFunction : StdFunction
    {
        [Comments("Returns the maximum value from a list of numbers.")]
        [Arguments(ParamNames = new[] { "values" }, ParamTypes = new[] { typeof(Number) })]
        [Returns(ReturnType = typeof(Number))]
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0)
                throw new ArgumentError("max expects at least one argument");

            try
            {
                return values.Max(v => Convert.ToDouble(v));
            }
            catch
            {
                throw new TypeError("max expects numeric arguments");
            }
        }
    }
}
