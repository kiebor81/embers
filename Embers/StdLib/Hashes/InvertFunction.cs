using Embers.Language;
using Embers.Exceptions;
using Embers.Annotations;
using System.Collections;

namespace Embers.StdLib.Hashes;

[StdLib("invert", TargetTypes = new[] { "Hash" })]
public class InvertFunction : StdFunction
{
    [Comments("Returns a new hash with keys and values swapped.")]
    [Arguments(ParamNames = new[] { "hash" }, ParamTypes = new[] { typeof(Hash) })]
    [Returns(ReturnType = typeof(object))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0 || values[0] == null)
            throw new ArgumentError("invert expects a hash argument");

        if (values[0] is IDictionary hash)
        {
            var result = new DynamicHash();
            foreach (DictionaryEntry entry in hash)
                result[entry.Value] = entry.Key;

            return result;
        }

        throw new TypeError("invert expects a hash");
    }
}
