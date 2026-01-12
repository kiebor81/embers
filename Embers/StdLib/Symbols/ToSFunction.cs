using Embers.Language;
using Embers.Exceptions;
using Embers.Annotations;

namespace Embers.StdLib.Symbols
{
    [StdLib("to_s", TargetType = "Symbol")]
    public class ToSFunction : StdFunction
    {
        [Comments("Returns the name of the symbol as a string.")]
        [Arguments(ParamNames = new[] { "symbol" }, ParamTypes = new[] { typeof(Symbol) })]
        [Returns(ReturnType = typeof(string))]
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values.Count != 1)
                throw new ArgumentError($"wrong number of arguments (given {values.Count - 1}, expected 0)");
            if (values[0] is not Symbol symbol)
                throw new TypeError("symbol must be a Symbol");

            return symbol.Name;
        }
    }
}
