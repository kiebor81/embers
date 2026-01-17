using Embers.Exceptions;
using Embers.Annotations;
using System.Collections;

namespace Embers.StdLib.Hashes;

[StdLib("to_a", TargetTypes = new[] { "Hash" })]
public class ToAFunction : StdFunction
{
    [Comments("Converts the hash into an array of [key, value] pairs.")]
    [Arguments(ParamNames = new[] { "hash" }, ParamTypes = new[] { typeof(Hash) })]
    [Returns(ReturnType = typeof(Array))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0 || values[0] == null)
            throw new ArgumentError("to_a expects a hash argument");

        if (values[0] is IDictionary hash)
        {
            var result = new DynamicArray();
            foreach (DictionaryEntry entry in hash)
            {
                var pair = new DynamicArray { entry.Key, entry.Value };
                result.Add(pair);
            }
            return result;
        }

        throw new TypeError("to_a expects a hash");
    }
}
