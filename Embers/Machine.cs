using System.Reflection;
using Embers.Exceptions;
using Embers.Functions;
using Embers.Security;
using Embers.Signals;
using Embers.StdLib.Conversion;

namespace Embers;

/// <summary>
/// Machine is the main entry point for the Embers language.
/// It holds the root context and provides methods to execute code.
/// The machine is responsible for managing the execution environment,
/// One can think of it as the runtime for the Embers language / representative of a ruby vm.
/// </summary>
public class Machine
{
    private static readonly string[] SupportedExtensions = [".ers", ".emb", ".rb", ".rs", ".txt"];
    /// <summary>
    /// The rootcontext
    /// </summary>
    private readonly Context rootcontext = new();
    private readonly IList<string> requirepaths = [];
    private readonly IList<string> required = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="Machine"/> class.
    /// </summary>
    public Machine()
    {
        requirepaths.Add(".");
        var basicobjectclass = new DynamicClass("BasicObject", null);
        var objectclass = new DynamicClass("Object", basicobjectclass);
        var moduleclass = new DynamicClass("Module", objectclass);
        var classclass = new DynamicClass("Class", moduleclass);

        rootcontext.SetLocalValue("BasicObject", basicobjectclass);
        rootcontext.SetLocalValue("Object", objectclass);
        rootcontext.SetLocalValue("Module", moduleclass);
        rootcontext.SetLocalValue("Class", classclass);

        basicobjectclass.SetClass(classclass);
        objectclass.SetClass(classclass);
        moduleclass.SetClass(classclass);
        classclass.SetClass(classclass);

        basicobjectclass.SetInstanceMethod("class", new LambdaFunction(GetClass));
        basicobjectclass.SetInstanceMethod("methods", new LambdaFunction(GetMethods));
        basicobjectclass.SetInstanceMethod("singleton_methods", new LambdaFunction(GetSingletonMethods));

        objectclass.SetInstanceMethod("inspect", new InspectFunction());

        moduleclass.SetInstanceMethod("superclass", new LambdaFunction(GetSuperClass));
        moduleclass.SetInstanceMethod("name", new LambdaFunction(GetName));
        moduleclass.SetInstanceMethod("include", new LambdaFunction(IncludeModule));
        moduleclass.SetInstanceMethod("class_variable_get", new LambdaFunction(ClassVariableGet));
        moduleclass.SetInstanceMethod("class_variable_set", new LambdaFunction(ClassVariableSet));

        classclass.SetInstanceMethod("new", new LambdaFunction(NewInstance));

        var fixnumNative = new FixnumClass(this);
        var floatNative = new FloatClass(this);
        var stringNative = new StringClass(this);
        var symbolNative = new SymbolClass(this);
        var nilNative = new NilClass(this);
        var falseNative = new FalseClass(this);
        var trueNative = new TrueClass(this);
        var arrayNative = new ArrayClass(this);
        var hashNative = new HashClass(this);
        var rangeNative = new RangeClass(this);
        var datetimeNative = new DateTimeClass(this);
        var jsonNative = new JsonClass(this);
        var procNative = new ProcClass(this);

        rootcontext.SetLocalValue("Fixnum", new NativeClassAdapter(classclass, "Fixnum", objectclass, null, fixnumNative));
        rootcontext.SetLocalValue("Float", new NativeClassAdapter(classclass, "Float", objectclass, null, floatNative));
        rootcontext.SetLocalValue("String", new NativeClassAdapter(classclass, "String", objectclass, null, stringNative));
        rootcontext.SetLocalValue("Symbol", new NativeClassAdapter(classclass, "Symbol", objectclass, null, symbolNative));
        rootcontext.SetLocalValue("NilClass", new NativeClassAdapter(classclass, "NilClass", objectclass, null, nilNative));
        rootcontext.SetLocalValue("FalseClass", new NativeClassAdapter(classclass, "FalseClass", objectclass, null, falseNative));
        rootcontext.SetLocalValue("TrueClass", new NativeClassAdapter(classclass, "TrueClass", objectclass, null, trueNative));
        rootcontext.SetLocalValue("Array", new NativeClassAdapter(classclass, "Array", objectclass, null, arrayNative));
        rootcontext.SetLocalValue("Hash", new NativeClassAdapter(classclass, "Hash", objectclass, null, hashNative));
        rootcontext.SetLocalValue("Range", new NativeClassAdapter(classclass, "Range", objectclass, null, rangeNative));
        rootcontext.SetLocalValue("DateTime", new NativeClassAdapter(classclass, "DateTime", objectclass, null, datetimeNative));
        rootcontext.SetLocalValue("JSON", new NativeClassAdapter(classclass, "JSON", objectclass, null, jsonNative));
        rootcontext.SetLocalValue("Proc", new NativeClassAdapter(classclass, "Proc", objectclass, null, procNative));

        rootcontext.Self = objectclass.CreateInstance();
        rootcontext.Self.Class.SetInstanceMethod("require", new RequireFunction(this));

        rootcontext.RegisterAllEmbersExceptions();
        rootcontext.RegisterAllStdLibFunctions();

    }

    /// <summary>
    /// Gets the root context.
    /// </summary>
    /// <value>
    /// The root context.
    /// </value>
    public Context RootContext { get { return rootcontext; } }

    /// <summary>
    /// Short-hand proxy for ExecuteText and ExecuteFile.
    /// </summary>
    /// <param name="argument">The executable argument.</param>
    public object Execute(string argument)
    {
        if (File.Exists(argument))
        {
            if (!HasSupportedExtension(argument))
                throw new UnsupportedFileError($"Unsupported file type: {Path.GetExtension(argument)}");
            return ExecuteFile(argument);
        }

        // Check if it looks like a file path but doesn't exist
        if (LooksLikeFilePath(argument))
        {
            if (Path.HasExtension(argument) && !HasSupportedExtension(argument))
                throw new UnsupportedFileError($"Unsupported file type: {Path.GetExtension(argument)}");
            throw new FileNotFoundException($"File not found: {argument}");
        }

        return ExecuteText(argument);
    }

    /// <summary>
    /// Determines if a string looks like a file path (contains path separators or file extensions).
    /// </summary>
    private static bool LooksLikeFilePath(string argument)
    {
        if (string.IsNullOrWhiteSpace(argument))
            return false;

        var trimmed = argument.Trim();

        // Check for common file extensions
        if (HasSupportedExtension(trimmed))
            return true;

        // Check if it's an absolute path (Windows: C:, D:, etc. or Unix: starts with /)
        if (Path.IsPathRooted(trimmed))
            return true;

        return false;
    }

    /// <summary>
    /// Determines if the file has a supported extension.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private static bool HasSupportedExtension(string path)
    {
        var ext = Path.GetExtension(path);
        if (string.IsNullOrEmpty(ext)) return false;

        foreach (var supported in SupportedExtensions)
        {
            if (ext.Equals(supported, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }


    /// <summary>
    /// Executes the text.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns></returns>
    public object ExecuteText(string text)
    {
        Parser parser = new(text);
        object result = null;

        try
        {
            for (var command = parser.ParseCommand(); command != null; command = parser.ParseCommand())
                result = command.Evaluate(rootcontext);
        }
        catch (ReturnSignal)
        {
            throw new InvalidOperationError("return can only be used inside methods");
        }

        return result;
    }

    /// <summary>
    /// Executes the file.
    /// </summary>
    /// <param name="filename">The filename.</param>
    /// <returns></returns>
    public object ExecuteFile(string filename)
    {
        string path = Path.GetDirectoryName(filename);

        requirepaths.Insert(0, path);

        try
        {
            return ExecuteText(File.ReadAllText(filename));
        }
        finally
        {
            requirepaths.RemoveAt(0);
        }
    }

    /// <summary>
    /// Mimics Ruby's require keyword as file ativity.
    /// </summary>
    /// <param name="filename">The filename.</param>
    /// <returns></returns>
    public bool RequireFile(string filename)
    {
        if (!Path.IsPathRooted(filename))
        {
            foreach (var path in requirepaths)
            {
                string newfilename = Path.Combine(path, filename);
                if (!File.Exists(newfilename))
                    newfilename = AppendFirstExistingExtension(newfilename);

                if (File.Exists(newfilename))
                {
                    filename = newfilename;
                    break;
                }
            }
        }
        else
        {
            string newfilename = Path.GetFullPath(filename);

            if (!File.Exists(newfilename))
                newfilename = AppendFirstExistingExtension(newfilename);

            filename = newfilename;
        }

        if (required.Contains(filename))
            return false;

        if (filename.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
        {
            Assembly.LoadFrom(filename);
            return true;
        }

        if (!HasSupportedExtension(filename))
            throw new UnsupportedFileError($"Unsupported file type: {Path.GetExtension(filename)}");

        ExecuteFile(filename);
        required.Add(filename);

        return true;
    }

    /// <summary>
    /// Appends the first existing permitted extension.
    /// </summary>
    /// <param name="basePath"></param>
    /// <returns></returns>
    private static string AppendFirstExistingExtension(string basePath)
    {
        foreach (var extension in SupportedExtensions)
        {
            var candidate = basePath + extension;
            if (File.Exists(candidate))
                return candidate;
        }

        var dllCandidate = basePath + ".dll";
        return File.Exists(dllCandidate) ? dllCandidate : basePath;
    }

    /// <summary>
    /// Executes the reader.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <returns></returns>
    public object ExecuteReader(TextReader reader)
    {
        Parser parser = new(reader);
        object result = null;

        try
        {
            for (var command = parser.ParseCommand(); command != null; command = parser.ParseCommand())
                result = command.Evaluate(rootcontext);
        }
        catch (ReturnSignal)
        {
            throw new InvalidOperationError("return can only be used inside methods");
        }

        return result;
    }

    /// <summary>
    /// Sets the type access policy.
    /// Allowed entries are a list of full type names that are allowed to be accessed.
    /// Provide allowed entries as a list of strings where final character '.' implies a namespace.
    /// </summary>
    /// <param name="allowedEntries">The allowed entries.</param>
    public void SetTypeAccessPolicy(IEnumerable<string> allowedEntries, SecurityMode mode = SecurityMode.WhitelistOnly) => TypeAccessPolicy.SetPolicy(allowedEntries, mode);

    /// <summary>
    /// Allows the type.
    /// </summary>
    /// <param name="fullTypeName">Full name of the type.</param>
    public void AllowType(string fullTypeName) => TypeAccessPolicy.AddType(fullTypeName);

    /// <summary>
    /// Allows the namespace.
    /// </summary>
    /// <param name="prefix">The prefix.</param>
    public void AllowNamespace(string prefix) => TypeAccessPolicy.AddNamespace(prefix);

    /// <summary>
    /// Clears the security policy.
    /// </summary>
    public void ClearSecurityPolicy() => TypeAccessPolicy.Clear();

    /// <summary>
    /// Creates a new instance of the class.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="context"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    private static object NewInstance(DynamicObject obj, Context context, IList<object> values)
    {
        var newobj = ((DynamicClass)obj).CreateInstance();

        var initialize = newobj.GetMethod("initialize");

        initialize?.Apply(newobj, context, values);

        return newobj;
    }

    /// <summary>
    /// Gets the name of the class.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="context"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    private static object GetName(DynamicObject obj, Context context, IList<object> values) => ((DynamicClass)obj).Name;

    /// <summary>
    /// Gets the super class.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="context"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    private static object GetSuperClass(DynamicObject obj, Context context, IList<object> values) => ((DynamicClass)obj).SuperClass;

    /// <summary>
    /// Gets the class.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="context"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    private static object GetClass(DynamicObject obj, Context context, IList<object> values) => obj.Class;

    /// <summary>
    /// Gets the methods.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="context"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    private static object GetMethods(DynamicObject obj, Context context, IList<object> values)
    {
        var result = new DynamicArray();

        for (var @class = obj.SingletonClass; @class != null; @class = @class.SuperClass)
        {
            var names = @class.GetOwnInstanceMethodNames();

            foreach (var name in names)
            {
                Symbol symbol = new(name);

                if (!result.Contains(symbol))
                    result.Add(symbol);
            }
        }

        return result;
    }

    /// <summary>
    /// Gets the singleton methods.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="context"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    private static object GetSingletonMethods(DynamicObject obj, Context context, IList<object> values)
    {
        var result = new DynamicArray();

        var names = obj.SingletonClass.GetOwnInstanceMethodNames();

        foreach (var name in names)
        {
            Symbol symbol = new(name);

            if (!result.Contains(symbol))
                result.Add(symbol);
        }

        return result;
    }

    /// <summary>
    /// Gets a class variable by name on a class or module.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="context"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    private static object? ClassVariableGet(DynamicObject obj, Context context, IList<object> values)
    {
        if (obj is not DynamicClass target)
            throw new TypeError("class_variable_get can only be called on classes or modules");

        if (values.Count != 1)
            throw new ArgumentError($"wrong number of arguments (given {values.Count}, expected 1)");

        string name = values[0] switch
        {
            Symbol symbol => symbol.Name,
            string text => text,
            _ => throw new TypeError("class_variable_get expects a symbol or string")
        };

        if (!name.StartsWith("@@", StringComparison.Ordinal))
            throw new NameError("invalid class variable name");

        return target.GetValue(name[2..]);
    }

    /// <summary>
    /// Sets a class variable by name on a class or module.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="context"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    private static object? ClassVariableSet(DynamicObject obj, Context context, IList<object> values)
    {
        if (obj is not DynamicClass target)
            throw new TypeError("class_variable_set can only be called on classes or modules");

        if (values.Count != 2)
            throw new ArgumentError($"wrong number of arguments (given {values.Count}, expected 2)");

        string name = values[0] switch
        {
            Symbol symbol => symbol.Name,
            string text => text,
            _ => throw new TypeError("class_variable_set expects a symbol or string")
        };

        if (!name.StartsWith("@@", StringComparison.Ordinal))
            throw new NameError("invalid class variable name");

        var value = values[1];
        target.SetValue(name[2..], value);
        return value;
    }

    /// <summary>
    /// Includes a module into a class or module.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="context"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    private static object IncludeModule(DynamicObject obj, Context context, IList<object> values)
    {
        if (obj is not DynamicClass target)
            throw new TypeError("include can only be called on classes or modules");

        if (values.Count != 1)
            throw new ArgumentError($"wrong number of arguments (given {values.Count}, expected 1)");

        if (values[0] is not DynamicClass module)
            throw new TypeError("include expects a module");

        if (context.RootContext.GetLocalValue("Module") is DynamicClass moduleClass && module.Class != moduleClass)
            throw new TypeError("include expects a module");

        target.IncludeModule(module);

        return target;
    }

}

