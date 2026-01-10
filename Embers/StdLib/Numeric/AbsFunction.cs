using Embers.Exceptions;
using Embers.Language;

namespace Embers.StdLib.Numeric
{
    /// <summary>
    /// Returns the absolute value of a number.
    /// </summary>
    [StdLib("abs", TargetTypes = new[] { "Fixnum", "Float" })]
    public class AbsFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                return null;

            var value = values[0];
            if (value is int i)
                return Math.Abs(i);
            if (value is double d)
                return Math.Abs(d);

            throw new ArgumentError("abs expects an int or double");
        }
    }
}
