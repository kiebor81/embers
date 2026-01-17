using Embers.Exceptions;
using Embers.Annotations;

namespace Embers.StdLib.Symbols;

[StdLib("to_sym", TargetType = "Symbol")]
public class ToSymFunction : StdFunction
{
    [Comments("Returns the symbol itself.")]
    [Arguments(ParamNames = new[] { "symbol" }, ParamTypes = new[] { typeof(Symbol) })]
    [Returns(ReturnType = typeof(Symbol))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values.Count != 1)
            throw new ArgumentError($"wrong number of arguments (given {values.Count - 1}, expected 0)");
        if (values[0] is not Symbol symbol)
            throw new TypeError("symbol must be a Symbol");

        return symbol;
    }
}
