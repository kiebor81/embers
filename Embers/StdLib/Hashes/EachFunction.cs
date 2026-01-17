using Embers.Exceptions;
using Embers.Annotations;
using System.Collections;
using Embers.Language.Primitive;

namespace Embers.StdLib.Hashes;

[StdLib("each", TargetType = "Hash")]
public class EachFunction : StdFunction
{
    [Comments("Iterates over each key-value pair in the hash, executing the provided block.")]
    [Arguments(ParamNames = new[] { "hash", "block" }, ParamTypes = new[] { typeof(Hash), typeof(Block) })]
    [Returns(ReturnType = typeof(Hash))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0 || values[0] == null)
            throw new ArgumentError("each expects a hash argument");

        if (context.Block == null)
            throw new ArgumentError("each expects a block");

        if (values[0] is IDictionary hash)
        {
            foreach (DictionaryEntry entry in hash)
                context.Block.Apply(self, context, [entry.Key, entry.Value]);
            return hash;
        }

        throw new TypeError("each expects a hash");
    }
}
