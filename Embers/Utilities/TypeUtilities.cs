using Embers.Exceptions;
using System.Reflection;

namespace Embers.Utilities;

/// <summary>
/// TypeUtilities provides methods for working with types, including retrieving types by name,
/// Its provides functionality to get types from loaded assemblies, parse enum values, and manage namespaces.
/// This provides Embers with a way to dynamically access types and their members at runtime and presents .NET interop capabilities.
/// </summary>
public class TypeUtilities
{
    private static bool referencedAssembliesLoaded = false;

    /// <summary>
    /// Attempts to get a type by name from the context or loaded assemblies.
    /// </summary>
    /// <param name="context">The context to search for the type.</param>
    /// <param name="name">The name of the type.</param>
    /// <returns>The type if found; otherwise, throws an exception.</returns>
    public static Type GetType(Context context, string name)
    {
        object obj = context.GetValue(name);

        if (obj != null && obj is Type)
            return (Type)obj;

        return GetType(name);
    }

    /// <summary>
    /// Attempts to get a type by name, returning null if not found.
    /// </summary>
    /// <param name="name">The name of the type.</param>
    /// <returns>The type if found; otherwise, null.</returns>
    public static Type? AsType(string name)
    {
        // Normalize :: to . for .NET type resolution
        // This allows both System.DateTime and System::DateTime syntax
        string normalizedName = name.Replace("::", ".");
        
        if (!Security.TypeAccessPolicy.IsAllowed(normalizedName))
            throw new TypeAccessError($"Access to type '{name}' is not permitted by the current type access policy.");

        Type type = Type.GetType(normalizedName);

        if (type != null)
            return type;

        type = GetTypeFromLoadedAssemblies(normalizedName);

        if (type != null)
            return type;

        type = GetTypeFromPartialNamedAssembly(normalizedName);

        if (type != null)
            return type;

        LoadReferencedAssemblies();

        type = GetTypeFromLoadedAssemblies(normalizedName);

        if (type != null)
            return type;

        return null;
    }

    /// <summary>
    /// Gets a type by name, throwing an exception if not found.
    /// </summary>
    /// <param name="name">The name of the type.</param>
    /// <returns>The type.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the type is not found.</exception>
    public static Type GetType(string name)
    {
        Type type = AsType(name);

        if (type != null)
            return type;

        throw new InvalidOperationException(string.Format("Unknown type '{0}'", name));
    }

    /// <summary>
    /// Gets the types by namespace.
    /// </summary>
    /// <param name="namespace">The namespace.</param>
    /// <returns></returns>
    public static ICollection<Type> GetTypesByNamespace(string @namespace)
    {
        IList<Type> types = [];

        LoadReferencedAssemblies();

        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            foreach (var type in assembly.GetTypes().Where(tp => tp.Namespace == @namespace))
                types.Add(type);

        return types;
    }

    /// <summary>
    /// Determines if a given name is a namespace.
    /// </summary>
    /// <param name="name">The name to check.</param>
    /// <returns>True if the name is a namespace; otherwise, false.</returns>
    public static bool IsNamespace(string name)
    {
        if (GetNamespaces().Contains(name))
            return true;

        return GetNamespaces().Any(n => n != null && n.StartsWith(name + "."));
    }

    /// <summary>
    /// Gets the names of public instance members of a type.
    /// </summary>
    /// <param name="type">The type to get member names from.</param>
    /// <returns>A list of member names.</returns>
    public static IList<string> GetNames(Type type) => [.. type.GetMembers(BindingFlags.Public | BindingFlags.Instance).Select(m => m.Name)];

    /// <summary>
    /// Gets the value of a static member (property or field) of a type.
    /// </summary>
    /// <param name="type">The type containing the member.</param>
    /// <param name="name">The name of the member.</param>
    /// <returns>The value of the static member.</returns>
    public static object GetValue(Type type, string name)
    {
        try
        {
            return type.InvokeMember(name, BindingFlags.FlattenHierarchy | BindingFlags.GetProperty | BindingFlags.GetField | BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, null, null);
        }
        catch
        {
            return type.GetMethod(name);
        }
    }

    /// <summary>
    /// Invokes a static member (method, property, or field) of a type.
    /// </summary>
    /// <param name="type">The type containing the member.</param>
    /// <param name="name">The name of the member to invoke.</param>
    /// <param name="parameters">The parameters to pass to the member.</param>
    /// <returns>The result of the member invocation.</returns>
    public static object InvokeTypeMember(Type type, string name, IList<object> parameters) => type.InvokeMember(name, BindingFlags.FlattenHierarchy | BindingFlags.GetProperty | BindingFlags.GetField | BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.Static, null, null, parameters?.ToArray());

    /// <summary>
    /// Parses an enum value from its name.
    /// </summary>
    /// <param name="type">The enum type.</param>
    /// <param name="name">The name of the enum value.</param>
    /// <returns>The enum value.</returns>
    /// <exception cref="ValueError">Thrown if the name is not a valid enum value.</exception>
    public static object ParseEnumValue(Type type, string name)
    {
        FieldInfo[] fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);

        for (int i = 0, count = fields.Length; i < count; i++)
        {
            FieldInfo fi = fields[i];
            if (fi.Name == name)
                return fi.GetValue(null);
        }

        throw new ValueError(string.Format("'{0}' is not a valid value of '{1}'", name, type.Name));
    }

    /// <summary>
    /// Gets all namespaces from loaded assemblies.
    /// </summary>
    /// <returns>A collection of namespace names.</returns>
    private static ICollection<string> GetNamespaces()
    {
        List<string> namespaces = [];

        LoadReferencedAssemblies();

        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            foreach (Type type in assembly.GetTypes())
                if (!namespaces.Contains(type.Namespace))
                    namespaces.Add(type.Namespace);

        return namespaces;
    }

    /// <summary>
    /// Gets a type from an assembly loaded by partial name.
    /// </summary>
    /// <param name="name">The full name of the type to retrieve.</param>
    /// <returns>The Type if found; otherwise, null.</returns>
    private static Type? GetTypeFromPartialNamedAssembly(string name)
    {
        int p = name.LastIndexOf(".");

        if (p < 0)
            return null;

        string assemblyName = name[..p];

        try
        {
            Assembly assembly = Assembly.LoadWithPartialName(assemblyName);

            return assembly.GetType(name);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets a type from the currently loaded assemblies.
    /// </summary>  
    /// <param name="name">The full name of the type to retrieve.</param>
    /// <returns>The Type if found; otherwise, null.</returns>
    private static Type? GetTypeFromLoadedAssemblies(string name)
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type type = assembly.GetType(name);

            if (type != null)
                return type;
        }

        return null;
    }

    /// <summary>
    /// Loads all referenced assemblies into the current AppDomain.
    /// </summary>
    private static void LoadReferencedAssemblies()
    {
        if (referencedAssembliesLoaded)
            return;

        List<string> loaded = [];

        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            loaded.Add(assembly.GetName().Name);

        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            LoadReferencedAssemblies(assembly, loaded);

        referencedAssembliesLoaded = true;
    }

    /// <summary>
    /// Recursively loads referenced assemblies.
    /// </summary>
    /// <param name="assembly">The assembly to load references from.</param>
    /// <param name="loaded">The list of already loaded assembly names.</param>
    /// <returns></returns>
    private static void LoadReferencedAssemblies(Assembly assembly, List<string> loaded)
    {
        foreach (AssemblyName referenced in assembly.GetReferencedAssemblies())
        {
            if (!loaded.Contains(referenced.Name))
            {
                loaded.Add(referenced.Name);
                try
                {
                    Assembly newassembly = Assembly.Load(referenced);
                    LoadReferencedAssemblies(newassembly, loaded);
                }
                catch (FileNotFoundException)
                {
                    // Ignore missing assemblies (e.g., System.Security.Permissions on .NET Core+)
                }
                catch (FileLoadException)
                {
                    // Ignore assemblies that can't be loaded
                }
                catch (BadImageFormatException)
                {
                    // Ignore assemblies that are not valid .NET assemblies
                }
            }
        }
    }
}
