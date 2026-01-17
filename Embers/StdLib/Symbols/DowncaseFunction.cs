using Embers.Annotations;

namespace Embers.StdLib.Symbols;

[StdLib("downcase", TargetType = "Symbol")]
public class DowncaseFunction : StdFunction
{
    [Comments("Converts the symbol to lowercase.")]
    [Arguments(ParamNames = new[] { "symbol" }, ParamTypes = new[] { typeof(Symbol) })]
    [Returns(ReturnType = typeof(Symbol))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values.Count != 1)
            throw new Exceptions.ArgumentError($"wrong number of arguments (given {values.Count - 1}, expected 0)");

        if (values[0] is not Symbol symbol)
            throw new Exceptions.TypeError("symbol must be a Symbol");

        return new Symbol(symbol.Name.ToLower());
    }
}
