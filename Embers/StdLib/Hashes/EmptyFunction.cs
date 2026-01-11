using Embers.Language;
using Embers.Exceptions;
using System.Collections;

namespace Embers.StdLib.Hashes
{
    [StdLib("empty?", TargetType = "Hash")]
    public class EmptyFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                throw new ArgumentError("empty? expects a hash argument");

            if (values[0] is IDictionary hash)
                return hash.Count == 0;

            throw new TypeError("empty? expects a hash");
        }
    }
}
