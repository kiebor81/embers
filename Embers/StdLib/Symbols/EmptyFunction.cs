using Embers.Language;

namespace Embers.StdLib.Symbols
{
    [StdLib("empty?", TargetType = "Symbol")]
    public class EmptyFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values.Count != 1)
                throw new Exceptions.ArgumentError($"wrong number of arguments (given {values.Count - 1}, expected 0)");

            var symbol = values[0] as Symbol;
            if (symbol == null)
                throw new Exceptions.TypeError("symbol must be a Symbol");

            return symbol.Name.Length == 0;
        }
    }
}
