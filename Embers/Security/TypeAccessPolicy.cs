namespace Embers.Security;

/// <summary>
/// Security modes for type access policy.
/// </summary>
public enum SecurityMode
{
    Unrestricted,
    WhitelistOnly
}

/// <summary>
/// Type access policy that controls which types can be accessed based on a whitelist.
/// This is used to prevent unauthorized access to potenitally dangerous types and methods
/// Security mode can be set to either unrestricted (allow all) or whitelist only (restrict access),
/// and is a host level concern.
/// </summary>
internal static class TypeAccessPolicy
{
    private static readonly object SyncLock = new();

    private static readonly HashSet<string> ExplicitTypeWhitelist = [];
    private static readonly List<string> NamespaceWhitelist = [];

    /// <summary>
    /// Gets or sets the current security mode.
    /// </summary>
    public static SecurityMode Mode { get; set; } = SecurityMode.Unrestricted;

    /// <summary>
    /// Sets the type access policy with the specified entries and mode.
    /// </summary>
    /// <param name="entries"></param>
    /// <param name="mode"></param>
    public static void SetPolicy(IEnumerable<string> entries, SecurityMode mode = SecurityMode.WhitelistOnly)
    {
        lock (SyncLock)
        {
            Clear();
            Mode = mode;

            foreach (var entry in entries)
            {
                if (entry.EndsWith(".*"))
                    AddNamespace(entry[..^2]);
                else
                    AddType(entry);
            }
        }
    }

    /// <summary>
    /// Sets the type access policy with the specified mode only.
    /// </summary>
    /// <param name="mode"></param>
    public static void SetPolicy(SecurityMode mode = SecurityMode.WhitelistOnly)
    {
        lock (SyncLock)
        {
            Clear();
            Mode = mode;
        }
    }

    /// <summary>
    /// Adds a single type to the type access policy.
    /// </summary>
    /// <param name="fullTypeName"></param>
    public static void AddType(string fullTypeName)
    {
        lock (SyncLock)
        {
            ExplicitTypeWhitelist.Add(fullTypeName);
        }
    }

    /// <summary>
    /// Adds multiple types to the type access policy.
    /// </summary>
    /// <param name="typeNames"></param>
    public static void AddTypes(IEnumerable<string> typeNames)
    {
        foreach (var name in typeNames)
            AddType(name);
    }

    /// <summary>
    /// Adds a namespace prefix to the type access policy.
    /// </summary>
    /// <param name="namespacePrefix"></param>
    public static void AddNamespace(string namespacePrefix)
    {
        lock (SyncLock)
        {
            NamespaceWhitelist.Add(namespacePrefix);
        }
    }

    /// <summary>
    /// Clears all entries from the type access policy.
    /// </summary>
    public static void Clear()
    {
        lock (SyncLock)
        {
            ExplicitTypeWhitelist.Clear();
            NamespaceWhitelist.Clear();
        }
    }

    /// <summary>
    /// Determines if the specified type is allowed based on the current security policy.
    /// </summary>
    /// <param name="fullTypeName"></param>
    /// <returns></returns>
    public static bool IsAllowed(string fullTypeName)
    {
        if (Mode == SecurityMode.Unrestricted)
            return true;

        return ExplicitTypeWhitelist.Contains(fullTypeName) ||
               NamespaceWhitelist.Any(fullTypeName.StartsWith);
    }
}

