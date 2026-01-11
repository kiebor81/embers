using Embers.Language;

namespace Embers.StdLib.Symbols
{
    [StdLib("intern", TargetType = "Symbol")]
    public class InternFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            return new ToSymFunction().Apply(self, context, values);
        }
    }
}
