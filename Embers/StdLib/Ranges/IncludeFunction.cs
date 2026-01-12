using Embers.Language;
using Embers.Annotations;

namespace Embers.StdLib.Ranges
{
    [StdLib("include?", TargetType = "Range")]
    public class IncludeFunction : StdFunction
    {
        [Comments("Returns true if the range includes the given value.")]
        [Arguments(ParamNames = new[] { "range", "value" }, ParamTypes = new[] { typeof(Language.Range), typeof(object) })]
        [Returns(ReturnType = typeof(Boolean))]
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values.Count != 2)
                throw new Exceptions.ArgumentError($"wrong number of arguments (given {values.Count - 1}, expected 1)");

            var range = values[0] as Language.Range;
            if (range == null)
                throw new Exceptions.TypeError("range must be a Range");

            // Accept both int and long
            if (values[1] is not int and not long)
                return false;
            
            int testValue = Convert.ToInt32(values[1]);

            // Get first and last values from range
            int first = 0;
            int last = 0;
            bool hasValues = false;

            foreach (var value in range)
            {
                if (!hasValues)
                {
                    first = value;
                    hasValues = true;
                }
                last = value;
            }

            if (!hasValues)
                return false;

            return testValue >= first && testValue <= last;
        }
    }
}
