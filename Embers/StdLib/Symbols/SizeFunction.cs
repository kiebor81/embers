using Embers.Language;

namespace Embers.StdLib.Symbols
{
    [StdLib("size", TargetType = "Symbol")]
    public class SizeFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            return new LengthFunction().Apply(self, context, values);
        }
    }
}
