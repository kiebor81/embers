using Embers.Exceptions;
using Embers.Language;

namespace Embers.StdLib.Strings
{
    /// <summary>
    /// Returns the reversed string or array.
    /// </summary>
    [StdLib("reverse", TargetType = "String")]
    public class ReverseFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                return null;

            var value = values[0];
            if (value is string s)
                return new string(s.Reverse().ToArray());
            if (value is IEnumerable<object> arr)
                return arr.Reverse().ToList();

            throw new ArgumentError("reverse expects a string or array");
        }
    }
}

