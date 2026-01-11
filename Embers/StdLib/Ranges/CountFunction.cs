using Embers.Language;

namespace Embers.StdLib.Ranges
{
    [StdLib("count", TargetType = "Range")]
    public class CountFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            // count is an alias for size in ranges
            return new SizeFunction().Apply(self, context, values);
        }
    }
}
