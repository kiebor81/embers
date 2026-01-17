using Embers.Exceptions;
using Embers.Annotations;
using System.Collections;

namespace Embers.StdLib.Hashes;

[StdLib("merge", TargetType = "Hash")]
public class MergeFunction : StdFunction
{
    [Comments("Merges two hashes into a new hash. If there are duplicate keys, values from the second hash overwrite those from the first.")]
    [Arguments(ParamNames = new[] { "hash1", "hash2" }, ParamTypes = new[] { typeof(Hash), typeof(Hash) })]
    [Returns(ReturnType = typeof(object))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count < 2 || values[0] == null || values[1] == null)
            throw new ArgumentError("merge expects two hash arguments");

        if (values[0] is IDictionary hash1 && values[1] is IDictionary hash2)
        {
            var result = new DynamicHash();
            
            // Copy first hash
            foreach (DictionaryEntry entry in hash1)
                result[entry.Key] = entry.Value;
            
            // Merge second hash (overwrites duplicate keys)
            foreach (DictionaryEntry entry in hash2)
                result[entry.Key] = entry.Value;
            
            return result;
        }

        throw new TypeError("merge expects hash arguments");
    }
}
