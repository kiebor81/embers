using Embers.Annotations;
using Embers.Language.Primitive;

namespace Embers.StdLib.Symbols;

[StdLib("size", TargetType = "Symbol")]
public class SizeFunction : StdFunction
{
    [Comments("Returns the size of the symbol's name.")]
    [Arguments(ParamNames = new[] { "symbol" }, ParamTypes = new[] { typeof(Symbol) })]
    [Returns(ReturnType = typeof(Number))]
    public override object Apply(DynamicObject self, Context context, IList<object> values) => new LengthFunction().Apply(self, context, values);
}
