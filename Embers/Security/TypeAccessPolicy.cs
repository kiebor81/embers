namespace Embers.Security;
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

    public static SecurityMode Mode { get; set; } = SecurityMode.Unrestricted;

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

    public static void AddType(string fullTypeName)
    {
        lock (SyncLock)
        {
            ExplicitTypeWhitelist.Add(fullTypeName);
        }
    }

    public static void AddTypes(IEnumerable<string> typeNames)
    {
        foreach (var name in typeNames)
            AddType(name);
    }

    public static void AddNamespace(string namespacePrefix)
    {
        lock (SyncLock)
        {
            NamespaceWhitelist.Add(namespacePrefix);
        }
    }

    public static void Clear()
    {
        lock (SyncLock)
        {
            ExplicitTypeWhitelist.Clear();
            NamespaceWhitelist.Clear();
        }
    }

    public static bool IsAllowed(string fullTypeName)
    {
        if (Mode == SecurityMode.Unrestricted)
            return true;

        return ExplicitTypeWhitelist.Contains(fullTypeName) ||
               NamespaceWhitelist.Any(prefix => fullTypeName.StartsWith(prefix));
    }
}

