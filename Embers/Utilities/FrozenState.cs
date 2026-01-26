using System.Runtime.CompilerServices;

namespace Embers.Utilities;

/// <summary>
/// Tracks the "frozen" state of objects without modifying their structure.
/// </summary>
internal static class FrozenState
{
    private sealed class Marker
    {
        public bool IsFrozen;
    }

    private static readonly ConditionalWeakTable<object, Marker> Frozen = new();

    /// <summary>
    /// Marks the target object as frozen.
    /// </summary>
    /// <param name="target"></param>
    public static void Freeze(object target)
    {
        if (target == null)
            return;

        var marker = Frozen.GetOrCreateValue(target);
        marker.IsFrozen = true;
    }

    /// <summary>
    /// Checks if the target object is frozen.
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public static bool IsFrozen(object? target)
    {
        if (target == null)
            return false;

        return Frozen.TryGetValue(target, out var marker) && marker.IsFrozen;
    }
}
