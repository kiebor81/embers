using Embers.Language;
using Embers.Annotations;

namespace Embers.StdLib.Ranges
{
    [StdLib("last", TargetType = "Range")]
    public class LastFunction : StdFunction
    {
        [Comments("Returns the last element of the range.")]
        [Arguments(ParamNames = new[] { "range", "block" }, ParamTypes = new[] { typeof(Language.Range), typeof(Block) })]
        [Returns(ReturnType = typeof(object))]
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values.Count != 1)
                throw new Exceptions.ArgumentError($"wrong number of arguments (given {values.Count - 1}, expected 0)");

            var range = values[0] as IEnumerable<int>;
            if (range == null)
                throw new Exceptions.TypeError("range must be a Range");

            int? lastValue = null;
            foreach (var value in range)
                lastValue = value;

            return lastValue;
        }
    }
}
