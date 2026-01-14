using Embers.Functions;
using Embers.Language;
using Embers.Host;
using Embers.StdLib;
using System.Reflection;

namespace Embers.Annotations;

/// <summary>
/// Scans all implementations of IFunction at runtime to extract documentation attributes.
/// Produces a dictionary indexed by class name with Tuple values of (comments, arguments, returns).
/// </summary>
public static class FunctionScanner
{
    /// <summary>
    /// Scans all loaded assemblies for IFunction implementations and extracts their documentation attributes.
    /// </summary>
    /// <returns>
    /// A dictionary where the key is the class name and the value is a Tuple of (comments, arguments, returns) strings.
    /// Uses string.Empty for attributes that are not present.
    /// </returns>
    public static Dictionary<string, (string Comments, string Arguments, string Returns)> ScanFunctionDocumentation()
    {
        var result = new Dictionary<string, (string, string, string)>();

        // Get all loaded assemblies
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            try
            {
                // Get all types that implement IFunction
                var functionTypes = assembly.GetTypes()
                    .Where(t => typeof(IFunction).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract && !HasScannerIgnoreAttribute(t));

                foreach (var type in functionTypes)
                {
                    // Get the Apply method
                    var applyMethod = type.GetMethod("Apply",
                        BindingFlags.Public | BindingFlags.Instance,
                        null,
                        [typeof(DynamicObject), typeof(Context), typeof(IList<object>)],
                        null);

                    if (applyMethod != null)
                    {
                        var comments = ExtractCommentsAttribute(applyMethod);
                        var arguments = ExtractArgumentsAttribute(applyMethod);
                        var returns = ExtractReturnsAttribute(applyMethod);

                        // Determine the key based on attributes or class name
                        var key = GetKeyForFunction(type);
                        result[key] = (comments, arguments, returns);
                    }
                }
            }
            catch (ReflectionTypeLoadException)
            {
                // Skip assemblies that can't be loaded
                continue;
            }
            catch
            {
                // Skip any other assembly loading errors
                continue;
            }
        }

        return result;
    }

    /// <summary>
    /// Scans all loaded assemblies for StdLib and Host functions and returns their names.
    /// </summary>
    public static (IReadOnlyCollection<string> StdLib, IReadOnlyCollection<string> Host) ScanFunctionNames()
    {
        var stdLib = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var host = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            try
            {
                var functionTypes = assembly.GetTypes()
                    .Where(t => typeof(IFunction).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract && !HasScannerIgnoreAttribute(t));

                foreach (var type in functionTypes)
                {
                    var hostAttr = type.GetCustomAttribute<HostFunctionAttribute>();
                    if (hostAttr != null && hostAttr.Names.Length > 0)
                    {
                        foreach (var name in hostAttr.Names)
                        {
                            host.Add(name);
                        }
                    }

                    var stdLibAttr = type.GetCustomAttribute<StdLibAttribute>();
                    if (stdLibAttr != null && stdLibAttr.Names.Length > 0)
                    {
                        foreach (var name in stdLibAttr.Names)
                        {
                            stdLib.Add(name);
                        }
                    }
                }
            }
            catch (ReflectionTypeLoadException)
            {
                continue;
            }
            catch
            {
                continue;
            }
        }

        return (stdLib.OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList(),
                host.OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList());
    }

    /// <summary>
    /// Scans the provided assemblies for Host functions and returns their names.
    /// </summary>
    public static IReadOnlyCollection<string> ScanHostFunctionNames(IEnumerable<Assembly> assemblies)
    {
        var host = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var assembly in assemblies)
        {
            try
            {
                var functionTypes = assembly.GetTypes()
                    .Where(t => typeof(IFunction).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract && !HasScannerIgnoreAttribute(t));

                foreach (var type in functionTypes)
                {
                    var hostAttr = type.GetCustomAttribute<HostFunctionAttribute>();
                    if (hostAttr == null || hostAttr.Names.Length == 0)
                        continue;

                    foreach (var name in hostAttr.Names)
                        host.Add(name);
                }
            }
            catch (ReflectionTypeLoadException)
            {
                continue;
            }
            catch
            {
                continue;
            }
        }

        return host.OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList();
    }

    /// <summary>
    /// Looks up documentation for a specific StdLib or Host function name.
    /// </summary>
    public static bool TryGetFunctionDocumentation(
        string functionName,
        out (string Name, string Kind, string Comments, string Arguments, string Returns) doc)
    {
        doc = default;
        if (string.IsNullOrWhiteSpace(functionName))
            return false;

        var target = functionName.Trim();
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            try
            {
                var functionTypes = assembly.GetTypes()
                    .Where(t => typeof(IFunction).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract && !HasScannerIgnoreAttribute(t));

                foreach (var type in functionTypes)
                {
                    var hostAttr = type.GetCustomAttribute<HostFunctionAttribute>();
                    if (hostAttr != null && hostAttr.Names.Length > 0)
                    {
                        if (hostAttr.Names.Any(n => string.Equals(n, target, StringComparison.OrdinalIgnoreCase)))
                        {
                            return TryBuildDoc(type, "Host", hostAttr.Names, out doc);
                        }
                    }

                    var stdLibAttr = type.GetCustomAttribute<StdLibAttribute>();
                    if (stdLibAttr != null && stdLibAttr.Names.Length > 0)
                    {
                        if (stdLibAttr.Names.Any(n => string.Equals(n, target, StringComparison.OrdinalIgnoreCase)))
                        {
                            return TryBuildDoc(type, "StdLib", stdLibAttr.Names, out doc);
                        }
                    }
                }
            }
            catch (ReflectionTypeLoadException)
            {
                continue;
            }
            catch
            {
                continue;
            }
        }

        return false;
    }

    private static bool TryBuildDoc(
        Type type,
        string kind,
        string[] names,
        out (string Name, string Kind, string Comments, string Arguments, string Returns) doc)
    {
        doc = default;

        var applyMethod = type.GetMethod("Apply",
            BindingFlags.Public | BindingFlags.Instance,
            null,
            [typeof(DynamicObject), typeof(Context), typeof(IList<object>)],
            null);

        if (applyMethod == null)
            return false;

        var comments = ExtractCommentsAttribute(applyMethod);
        var arguments = ExtractArgumentsAttribute(applyMethod);
        var returns = ExtractReturnsAttribute(applyMethod);

        doc = (string.Join(", ", names), kind, comments, arguments, returns);
        return true;
    }

    /// <summary>
    /// Extracts the CommentsAttribute from a method and returns a formatted string.
    /// </summary>
    private static string ExtractCommentsAttribute(MethodInfo method)
    {
        var attribute = method.GetCustomAttribute<CommentsAttribute>();
        if (attribute == null)
            return string.Empty;

        if (attribute.Comments == null || attribute.Comments.Length == 0)
            return string.Empty;

        return string.Join(" ", attribute.Comments);
    }

    /// <summary>
    /// Extracts the ArgumentsAttribute from a method and returns a formatted string.
    /// </summary>
    private static string ExtractArgumentsAttribute(MethodInfo method)
    {
        var attribute = method.GetCustomAttribute<ArgumentsAttribute>();
        if (attribute == null)
            return string.Empty;

        if (attribute.ParamNames == null || attribute.ParamNames.Length == 0)
            return string.Empty;

        var argParts = new List<string>();
        for (int i = 0; i < attribute.ParamNames.Length; i++)
        {
            var name = attribute.ParamNames[i];
            var type = i < attribute.ParamTypes.Length ? attribute.ParamTypes[i].Name : "object";
            argParts.Add($"{name}:{type}");
        }

        return string.Join(", ", argParts);
    }

    /// <summary>
    /// Extracts the ReturnsAttribute from a method and returns a formatted string.
    /// </summary>
    private static string ExtractReturnsAttribute(MethodInfo method)
    {
        var attribute = method.GetCustomAttribute<ReturnsAttribute>();
        if (attribute == null)
            return string.Empty;

        if (attribute.ReturnType == null)
            return string.Empty;

        return attribute.ReturnType.Name;
    }

    /// <summary>
    /// Checks if a type has the ScannerIgnoreAttribute (not inherited).
    /// </summary>
    private static bool HasScannerIgnoreAttribute(Type type) => type.GetCustomAttribute<ScannerIgnoreAttribute>(inherit: false) != null;

    /// <summary>
    /// Determines the dictionary key for a function based on its attributes.
    /// Checks for HostFunctionAttribute or StdLibAttribute and uses their Names property.
    /// Falls back to the class name if neither attribute is present.
    /// </summary>
    private static string GetKeyForFunction(Type type)
    {
        // Check for HostFunctionAttribute first
        var hostAttr = type.GetCustomAttribute<HostFunctionAttribute>();
        if (hostAttr != null && hostAttr.Names.Length > 0)
        {
            return string.Join(", ", hostAttr.Names);
        }

        // Check for StdLibAttribute
        var stdLibAttr = type.GetCustomAttribute<StdLibAttribute>();
        if (stdLibAttr != null && stdLibAttr.Names.Length > 0)
        {
            return string.Join(", ", stdLibAttr.Names);
        }

        // Fall back to class name
        return type.Name;
    }
}

