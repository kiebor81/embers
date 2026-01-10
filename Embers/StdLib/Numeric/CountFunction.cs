using Embers.Language;
using Embers.Exceptions;

namespace Embers.StdLib.Numeric
{
    [StdLib("count", TargetTypes = new[] { "Fixnum", "Float" })]
    public class CountFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count < 2 || values[0] == null || values[1] == null)
                throw new ArgumentError("count expects a string and a substring");

            if (values[0] is string s && values[1] is string sub)
                return (s.Length - s.Replace(sub, "").Length) / sub.Length;

            throw new TypeError("count expects string arguments");
        }
    }
}
