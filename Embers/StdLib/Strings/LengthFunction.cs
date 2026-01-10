using Embers.Exceptions;
using Embers.Language;

namespace Embers.StdLib.Strings
{
    /// <summary>
    /// Returns the length of a string or array.
    /// </summary>
    [StdLib("length", "len", TargetType = "String")]
    public class LengthFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                return 0;

            var value = values[0];
            if (value is string s)
                return s.Length;
            if (value is IEnumerable<object> arr)
                return arr.Count();

            throw new ArgumentError("length expects a string or array");
        }
    }
}

