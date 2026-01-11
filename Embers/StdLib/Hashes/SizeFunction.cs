using Embers.Language;
using Embers.Exceptions;
using System.Collections;

namespace Embers.StdLib.Hashes
{
    [StdLib("size", TargetType = "Hash")]
    public class SizeFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                throw new ArgumentError("size expects a hash argument");

            if (values[0] is IDictionary hash)
                return hash.Count;

            throw new TypeError("size expects a hash");
        }
    }
}
