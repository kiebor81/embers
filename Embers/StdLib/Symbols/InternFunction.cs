using Embers.Language;
using Embers.Annotations;

namespace Embers.StdLib.Symbols
{
    [StdLib("intern", TargetType = "Symbol")]
    public class InternFunction : StdFunction
    {
        [Comments("Converts a string to a symbol.")]
        [Arguments(ParamNames = new[] { "string" }, ParamTypes = new[] { typeof(string) })]
        [Returns(ReturnType = typeof(Symbol))]
        public override object Apply(DynamicObject self, Context context, IList<object> values) => new ToSymFunction().Apply(self, context, values);
    }
}
