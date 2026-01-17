using Embers.Exceptions;
using Embers.Annotations;
using Embers.Language.Primitive;

namespace Embers.StdLib.Symbols;

[StdLib("empty?", TargetType = "Symbol")]
public class EmptyFunction : StdFunction
{
    [Comments("Checks if the symbol is empty.")]
    [Arguments(ParamNames = new[] { "symbol" }, ParamTypes = new[] { typeof(Symbol) })]
    [Returns(ReturnType = typeof(Boolean))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values.Count != 1)
            throw new ArgumentError($"wrong number of arguments (given {values.Count - 1}, expected 0)");

        if (values[0] is not Symbol symbol)
            throw new TypeError("symbol must be a Symbol");

        return symbol.Name.Length == 0;
    }
}
