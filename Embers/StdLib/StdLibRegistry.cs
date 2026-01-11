using Embers.Security;
using System.Reflection;

namespace Embers.StdLib
{
    /// <summary>
    /// StdLibRegistry provides reflection-based discovery and registration of StdLib functions.
    /// It automatically discovers functions marked with [StdLib] attribute and registers them
    /// either globally or on specific target types.
    /// </summary>
    internal static class StdLibRegistry
    {
        private static readonly Dictionary<string, Dictionary<string, Type>> TypeMethodCache = new();
        private static readonly object InitLock = new();
        private static volatile bool isInitialized = false;

        /// <summary>
        /// Initializes the registry by discovering all StdLib functions via reflection.
        /// This should be called once during Machine initialization.
        /// Thread-safe implementation using double-checked locking pattern.
        /// </summary>
        internal static void Initialize()
        {
            if (isInitialized)
                return;

            lock (InitLock)
            {
                // Double-check inside lock
                if (isInitialized)
                    return;

                var baseType = typeof(StdFunction);
                var assembly = baseType.Assembly;

                foreach (var type in assembly.GetTypes())
                {
                    if (!type.IsClass || type.IsAbstract || !baseType.IsAssignableFrom(type))
                        continue;

                    var attr = type.GetCustomAttribute<StdLibAttribute>();
                    if (attr == null)
                        continue;

                    // Add to security whitelist
                    TypeAccessPolicy.AddType(type.FullName);

                    // Determine target types (null/empty = global, otherwise specific types)
                    var targetTypes = attr.TargetTypes != null && attr.TargetTypes.Length > 0
                        ? attr.TargetTypes
                        : new[] { string.Empty };

                    // Register on all target types
                    foreach (var targetType in targetTypes)
                    {
                        var key = targetType ?? string.Empty;

                        if (!TypeMethodCache.ContainsKey(key))
                            TypeMethodCache[key] = new Dictionary<string, Type>();

                        // Register all names for this function
                        foreach (var name in attr.Names)
                        {
                            TypeMethodCache[key][name] = type;
                        }
                    }
                }

                isInitialized = true;
            }
        }

        /// <summary>
        /// Gets a StdLib function instance for a specific method name on a target type.
        /// </summary>
        /// <param name="targetType">Target type name (e.g., "Array", "String") or null for global</param>
        /// <param name="methodName">Method name to look up</param>
        /// <returns>StdFunction instance or null if not found</returns>
        internal static StdFunction? GetMethod(string? targetType, string methodName)
        {
            if (!isInitialized)
                Initialize();

            targetType ??= string.Empty;

            lock (InitLock)
            {
                // Try target-specific lookup first
                if (TypeMethodCache.TryGetValue(targetType, out var methods))
                {
                    if (methods.TryGetValue(methodName, out var funcType))
                    {
                        return (StdFunction)Activator.CreateInstance(funcType);
                    }
                }

                // Fallback to global methods if not found on specific type
                if (!string.IsNullOrEmpty(targetType) && TypeMethodCache.TryGetValue(string.Empty, out var globalMethods))
                {
                    if (globalMethods.TryGetValue(methodName, out var funcType))
                    {
                        return (StdFunction)Activator.CreateInstance(funcType);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets all method names registered for a specific target type.
        /// </summary>
        /// <param name="targetType">Target type name or null for global</param>
        /// <returns>List of method names</returns>
        internal static IEnumerable<string> GetMethodNames(string? targetType)
        {
            if (!isInitialized)
                Initialize();

            targetType ??= string.Empty;

            lock (InitLock)
            {
                if (TypeMethodCache.TryGetValue(targetType, out var methods))
                    return methods.Keys.ToList(); // Return copy to avoid collection modification
            }

            return Enumerable.Empty<string>();
        }

        /// <summary>
        /// Registers global StdLib functions on the root context (existing behavior).
        /// </summary>
        internal static void RegisterGlobalFunctions(Context context)
        {
            if (!isInitialized)
                Initialize();

            lock (InitLock)
            {
                if (!TypeMethodCache.TryGetValue(string.Empty, out var globalMethods))
                    return;

                foreach (var kvp in globalMethods)
                {
                    var instance = (StdFunction)Activator.CreateInstance(kvp.Value);
                    foreach (var name in kvp.Value.GetCustomAttribute<StdLibAttribute>().Names)
                    {
                        context.Self.Class.SetInstanceMethod(name, instance);
                    }
                }
            }
        }
    }
}
