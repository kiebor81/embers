using Embers.Language;
using Embers.Exceptions;
using Embers.Annotations;
using System.Collections;

namespace Embers.StdLib.Hashes;

[StdLib("values", TargetType = "Hash")]
public class ValuesFunction : StdFunction
{
    [Comments("Returns an array of all values in the hash.")]
    [Arguments(ParamNames = new[] { "hash" }, ParamTypes = new[] { typeof(Hash) })]
    [Returns(ReturnType = typeof(Array))]
    public override object Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0 || values[0] == null)
            throw new ArgumentError("values expects a hash argument");

        if (values[0] is IDictionary hash)
        {
            var result = new DynamicArray();
            foreach (var value in hash.Values)
                result.Add(value);
            return result;
        }

        throw new TypeError("values expects a hash");
    }
}
