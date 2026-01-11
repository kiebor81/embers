using Embers.Language;

namespace Embers.StdLib.Ranges
{
    [StdLib("cover?", TargetType = "Range")]
    public class CoverFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            // cover? is an alias for include? in ranges
            return new IncludeFunction().Apply(self, context, values);
        }
    }
}
