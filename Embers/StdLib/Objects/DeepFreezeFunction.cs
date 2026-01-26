using System.Collections;
using System.Collections.Generic;
using Embers.Annotations;
using Embers.Exceptions;
using Embers.Language.Dynamic;
using Embers.Utilities;

namespace Embers.StdLib.Objects;

[StdLib("deep_freeze", TargetTypes = new[] { "Array", "Hash" })]
public class DeepFreezeFunction : StdFunction
{
    [Comments("Recursively freezes nested arrays and hashes.")]
    [Returns(ReturnType = typeof(object))]
    public override object? Apply(DynamicObject self, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0)
            throw new ArgumentError("deep_freeze expects a receiver");

        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
        DeepFreeze(values[0], visited);
        return values[0];
    }

    private static void DeepFreeze(object? target, HashSet<object> visited)
    {
        if (target == null)
            return;

        if (!visited.Add(target))
            return;

        if (target is IDictionary dict)
        {
            FreezeDictionary(dict);

            foreach (DictionaryEntry entry in dict)
            {
                DeepFreeze(entry.Key, visited);
                DeepFreeze(entry.Value, visited);
            }

            return;
        }

        if (target is IList list)
        {
            FreezeList(list);

            foreach (var item in list)
                DeepFreeze(item, visited);
        }
    }

    private static void FreezeList(IList list)
    {
        if (list is DynamicArray dynArray)
            dynArray.Freeze();
        else
            FrozenState.Freeze(list);
    }

    private static void FreezeDictionary(IDictionary dict)
    {
        if (dict is DynamicHash dynHash)
            dynHash.Freeze();
        else
            FrozenState.Freeze(dict);
    }
}
