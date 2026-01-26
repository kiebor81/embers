using Embers.Exceptions;
using Embers.Annotations;
using Embers.Utilities;
using System.Collections;

namespace Embers.StdLib.Hashes;

[StdLib("merge!", "update", TargetTypes = new[] { "Hash" })]
public class MergeBangFunction : StdFunction
{
    [Comments("Merges another hash into this hash, overwriting duplicate keys.")]
    [Arguments(ParamNames = new[] { "hash", "other" }, ParamTypes = new[] { typeof(Hash), typeof(Hash) })]
    [Returns(ReturnType = typeof(object))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count < 2 || values[0] == null || values[1] == null)
            throw new ArgumentError("merge! expects two hash arguments");

        if (values[0] is IDictionary target && values[1] is IDictionary other)
        {
            if ((target is DynamicHash dynHash && dynHash.IsFrozen) || FrozenState.IsFrozen(target))
                throw new FrozenError("can't modify frozen Hash");

            foreach (DictionaryEntry entry in other)
                target[entry.Key] = entry.Value;

            return values[0];
        }

        throw new TypeError("merge! expects hash arguments");
    }
}
