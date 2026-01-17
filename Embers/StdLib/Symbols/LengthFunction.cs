using Embers.Exceptions;
using Embers.Annotations;
using Embers.Language.Primitive;

namespace Embers.StdLib.Symbols;

[StdLib("length", TargetType = "Symbol")]
public class LengthFunction : StdFunction
{
    [Comments("Returns the length of the symbol's name.")]
    [Arguments(ParamNames = new[] { "symbol" }, ParamTypes = new[] { typeof(Symbol) })]
    [Returns(ReturnType = typeof(Number))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values.Count != 1)
            throw new ArgumentError($"wrong number of arguments (given {values.Count - 1}, expected 0)");

        if (values[0] is not Symbol symbol)
            throw new TypeError("symbol must be a Symbol");

        return symbol.Name.Length;
    }
}
