using Embers.Language;
using Embers.Exceptions;
using Embers.Annotations;
using System.Collections;

namespace Embers.StdLib.Hashes;

[StdLib("reject", TargetType = "Hash")]
public class RejectFunction : StdFunction
{
    [Comments("Creates a new hash excluding the key-value pairs for which the provided block returns true.")]
    [Arguments(ParamNames = new[] { "hash", "block" }, ParamTypes = new[] { typeof(Hash), typeof(Block) })]
    [Returns(ReturnType = typeof(Hash))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0 || values[0] == null)
            throw new ArgumentError("reject expects a hash argument");

        if (context.Block == null)
            throw new ArgumentError("reject expects a block");

        if (values[0] is IDictionary hash)
        {
            var result = new DynamicHash();
            foreach (DictionaryEntry entry in hash)
            {
                var blockResult = context.Block.Apply(self, context, [entry.Key, entry.Value]);
                if (!Predicates.IsTrue(blockResult))
                    result[entry.Key] = entry.Value;
            }
            return result;
        }

        throw new TypeError("reject expects a hash");
    }
}
