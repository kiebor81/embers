using Embers.Language;
using Embers.Annotations;

namespace Embers.StdLib.Symbols
{
    [StdLib("capitalize", TargetType = "Symbol")]
    public class CapitalizeFunction : StdFunction
    {
        [Comments("Capitalizes the symbol (first letter uppercase, rest lowercase).")]
        [Arguments(ParamNames = new[] { "symbol" }, ParamTypes = new[] { typeof(Symbol) })]
        [Returns(ReturnType = typeof(Symbol))]
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values.Count != 1)
                throw new Exceptions.ArgumentError($"wrong number of arguments (given {values.Count - 1}, expected 0)");

            var symbol = values[0] as Symbol;
            if (symbol == null)
                throw new Exceptions.TypeError("symbol must be a Symbol");

            if (string.IsNullOrEmpty(symbol.Name))
                return symbol;

            var capitalized = char.ToUpper(symbol.Name[0]) + symbol.Name[1..].ToLower();
            return new Symbol(capitalized);
        }
    }
}
