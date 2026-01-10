using Embers.Language;
using Embers.Exceptions;

namespace Embers.StdLib.Numeric
{
    /// <summary>
    /// Returns the maximum value from a list of numbers.
    /// </summary>
    [StdLib("max", TargetTypes = new[] { "Fixnum", "Float" })]
    public class MaxFunction : StdFunction
    {
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
