using Embers.Language;
using Embers.Exceptions;
using System.Collections;

namespace Embers.StdLib.Hashes
{
    [StdLib("keys", TargetType = "Hash")]
    public class KeysFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                throw new ArgumentError("keys expects a hash argument");

            if (values[0] is IDictionary hash)
            {
                var result = new DynamicArray();
                foreach (var key in hash.Keys)
                    result.Add(key);
                return result;
            }

            throw new TypeError("keys expects a hash");
        }
    }
}
