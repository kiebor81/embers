using Embers.Language;
using Embers.Exceptions;
using System.Collections;

namespace Embers.StdLib.Hashes
{
    [StdLib("map", TargetType = "Hash")]
    public class MapFunction : StdFunction
    {
        public override object Apply(DynamicObject self, Context context, IList<object> values)
        {
            if (values == null || values.Count == 0 || values[0] == null)
                throw new ArgumentError("map expects a hash argument");

            if (context.Block == null)
                throw new ArgumentError("map expects a block");

            if (values[0] is IDictionary hash)
            {
                var result = new DynamicArray();
                foreach (DictionaryEntry entry in hash)
                    result.Add(context.Block.Apply(self, context, [entry.Key, entry.Value]));
                return result;
            }

            throw new TypeError("map expects a hash");
        }
    }
}
