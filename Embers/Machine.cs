using System.Reflection;
using System.Runtime.CompilerServices;
using Embers.Exceptions;
using Embers.Functions;
using Embers.Security;
using Embers.Signals;

namespace Embers;

/// <summary>
/// Machine is the main entry point for the Embers language.
/// It holds the root context and provides methods to execute code.
/// The machine is responsible for managing the execution environment,
/// One can think of it as the runtime for the Embers language / representative of a ruby vm.
/// </summary>
public partial class Machine
{
    private static readonly ConditionalWeakTable<Context, HashSet<string>> MethodMissingGuards = new();

    /// <summary>
    /// Custom supported file extensions.
    /// </summary>
    private string[] customSupportedExtensions = [];
    private static readonly string[] SupportedExtensions = [".ers", ".emb", ".rb", ".rs", ".txt"];
    /// <summary>
    /// The rootcontext
    /// </summary>
    private readonly Context rootcontext = new();
    private readonly IList<string> requirepaths = [];
    private readonly IList<string> required = [];
    private static int anonymousStructIndex;

    internal record struct CoreClasses(
        DynamicClass BasicObject,
        DynamicClass Object,
        DynamicClass Module,
        DynamicClass Class);

    internal record struct CoreModules(
        DynamicClass Enumerable,
        DynamicClass Comparable);

    /// <summary>
    /// Initializes a new instance of the <see cref="Machine"/> class.
    /// </summary>
    public Machine()
    {
        requirepaths.Add(".");
        var coreClasses = this.RegisterCoreClasses();
        var coreModules = this.RegisterCoreModules(coreClasses);
        this.RegisterCoreMethods(coreClasses);
        this.RegisterNativeClasses(coreClasses);
        this.RegisterModuleMixins(coreModules);
        this.RegisterRootContextSelf(coreClasses);
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
    /// Gets the supported file extensions.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> GetSupportedExtensions()
    {
        foreach (var ext in SupportedExtensions)
            yield return ext;

        foreach (var ext in customSupportedExtensions)
            yield return ext;
    }

    /// <summary>
    /// Clears all supported file extensions.
    /// </summary>
    public void ClearSupportedExtensions()
    {
        customSupportedExtensions = [];
    }

    /// <summary>
    /// Sets the supported file extensions.
    /// </summary>
    /// <param name="extensions"></param>
    public void SetSupportedExtensions(IEnumerable<string> extensions)
    {
        customSupportedExtensions = [.. extensions.Select(ext => ext.StartsWith('.') ? ext : "." + ext)];
    }

    /// <summary>
    /// Adds a supported file extension.
    /// </summary>
    /// <param name="extension"></param>
    public void AddSupportedExtension(string extension)
    {
        if (!extension.StartsWith('.'))
            extension = "." + extension;

        var extensionsList = customSupportedExtensions.ToList();
        if (!extensionsList.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            extensionsList.Add(extension);
            customSupportedExtensions = [.. extensionsList];
        }
    }

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
    private bool LooksLikeFilePath(string argument)
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
    private bool HasSupportedExtension(string path)
    {
        var ext = Path.GetExtension(path);
        if (string.IsNullOrEmpty(ext)) return false;

        foreach (var supported in SupportedExtensions)
        {
            if (ext.Equals(supported, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        foreach (var supported in customSupportedExtensions)
        {
            if (ext.Equals(supported, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Executes the text. Embers will parse and execute the provided text.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns></returns>
    public object ExecuteText(string text)
    {
        Parser parser = new(text);
        object? result = null;

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
    /// Executes the file. Embers code is read from the file and executed.
    /// </summary>
    /// <param name="filename">The filename.</param>
    /// <returns></returns>
    public object ExecuteFile(string filename)
    {
        string? path = Path.GetDirectoryName(filename);

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
    /// Executes the Embers reader.
    /// This allows executing from any TextReader source, e.g., StringReader, StreamReader, etc.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <returns></returns>
    public object ExecuteReader(TextReader reader)
    {
        Parser parser = new(reader);
        object? result = null;

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
    private string AppendFirstExistingExtension(string basePath)
    {
        foreach (var extension in SupportedExtensions)
        {
            var candidate = basePath + extension;
            if (File.Exists(candidate))
                return candidate;
        }

        foreach (var extension in customSupportedExtensions)
        {
            var candidate = basePath + extension;
            if (File.Exists(candidate))
                return candidate;
        }

        var dllCandidate = basePath + ".dll";
        return File.Exists(dllCandidate) ? dllCandidate : basePath;
    }

    /// <summary>
    /// Sets the type access policy.
    /// Allowed entries are a list of full type names that are allowed to be accessed.
    /// Provide allowed entries as a list of strings where final character '.' implies a namespace.
    /// </summary>
    /// <param name="allowedEntries">The allowed entries.</param>
    public void SetTypeAccessPolicy(IEnumerable<string> allowedEntries, SecurityMode mode = SecurityMode.WhitelistOnly)
    {
        if (allowedEntries == null || !allowedEntries.Any())
            TypeAccessPolicy.SetPolicy(mode);
        else
            TypeAccessPolicy.SetPolicy(allowedEntries, mode);
    }

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
    internal static object NewInstance(DynamicObject obj, Context context, IList<object> values)
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
    internal static object GetName(DynamicObject obj, Context context, IList<object> values) => ((DynamicClass)obj).Name;

    /// <summary>
    /// Gets the super class.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="context"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    internal static object GetSuperClass(DynamicObject obj, Context context, IList<object> values) => ((DynamicClass)obj).SuperClass;

    /// <summary>
    /// Gets the class.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="context"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    internal static object GetClass(DynamicObject obj, Context context, IList<object> values) => obj.Class;

    /// <summary>
    /// Gets the methods.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="context"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    internal static object GetMethods(DynamicObject obj, Context context, IList<object> values)
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
    internal static object GetSingletonMethods(DynamicObject obj, Context context, IList<object> values)
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
    internal static object? ClassVariableGet(DynamicObject obj, Context context, IList<object> values)
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
    internal static object? ClassVariableSet(DynamicObject obj, Context context, IList<object> values)
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
    /// Gets an instance variable by name on an object.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="context"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    internal static object? InstanceVariableGet(DynamicObject obj, Context context, IList<object> values)
    {
        if (values.Count != 1)
            throw new ArgumentError($"wrong number of arguments (given {values.Count}, expected 1)");

        string name = values[0] switch
        {
            Symbol symbol => symbol.Name,
            string text => text,
            _ => throw new TypeError("instance_variable_get expects a symbol or string")
        };

        if (!name.StartsWith("@", StringComparison.Ordinal))
            throw new NameError("invalid instance variable name");

        return obj.GetValue(name[1..]);
    }

    /// <summary>
    /// Sets an instance variable by name on an object.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="context"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    internal static object? InstanceVariableSet(DynamicObject obj, Context context, IList<object> values)
    {
        if (values.Count != 2)
            throw new ArgumentError($"wrong number of arguments (given {values.Count}, expected 2)");

        string name = values[0] switch
        {
            Symbol symbol => symbol.Name,
            string text => text,
            _ => throw new TypeError("instance_variable_set expects a symbol or string")
        };

        if (!name.StartsWith("@", StringComparison.Ordinal))
            throw new NameError("invalid instance variable name");

        if (obj.IsFrozen)
            throw new FrozenError("can't modify frozen object");

        var value = values[1];
        obj.SetValue(name[1..], value);
        return value;
    }

    /// <summary>
    /// Lists instance variables on an object.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="context"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    internal static object InstanceVariables(DynamicObject obj, Context context, IList<object> values)
    {
        if (values.Count != 0)
            throw new ArgumentError($"wrong number of arguments (given {values.Count}, expected 0)");

        var result = new DynamicArray();
        foreach (var key in obj.GetValues().Keys)
            result.Add(new Symbol("@" + key));

        return result;
    }

    /// <summary>
    /// Freezes the object.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="context"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentError"></exception>
    internal static object FreezeObject(DynamicObject obj, Context context, IList<object> values)
    {
        if (values.Count != 0)
            throw new ArgumentError($"wrong number of arguments (given {values.Count}, expected 0)");

        obj.Freeze();
        return obj;
    }

    /// <summary>
    /// Checks if the object is frozen.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="context"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentError"></exception>
    internal static object IsFrozenObject(DynamicObject obj, Context context, IList<object> values)
    {
        if (values.Count != 0)
            throw new ArgumentError($"wrong number of arguments (given {values.Count}, expected 0)");

        return obj.IsFrozen;
    }

    /// <summary>
    /// Gets a constant by name on a class or module.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="context"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    internal static object? ConstGet(DynamicObject obj, Context context, IList<object> values)
    {
        if (obj is not DynamicClass target)
            throw new TypeError("const_get can only be called on classes or modules");

        if (values.Count != 1)
            throw new ArgumentError($"wrong number of arguments (given {values.Count}, expected 1)");

        string name = values[0] switch
        {
            Symbol symbol => symbol.Name,
            string text => text,
            _ => throw new TypeError("const_get expects a symbol or string")
        };

        if (!Predicates.IsConstantName(name))
            throw new NameError("invalid constant name");

        if (!target.Constants.HasLocalValue(name))
            throw new NameError($"unitialized constant {target.Name}::{name}");

        return target.Constants.GetLocalValue(name);
    }

    /// <summary>
    /// Sets a constant by name on a class or module.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="context"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    internal static object? ConstSet(DynamicObject obj, Context context, IList<object> values)
    {
        if (obj is not DynamicClass target)
            throw new TypeError("const_set can only be called on classes or modules");

        if (values.Count != 2)
            throw new ArgumentError($"wrong number of arguments (given {values.Count}, expected 2)");

        string name = values[0] switch
        {
            Symbol symbol => symbol.Name,
            string text => text,
            _ => throw new TypeError("const_set expects a symbol or string")
        };

        if (!Predicates.IsConstantName(name))
            throw new NameError("invalid constant name");

        var value = values[1];
        target.Constants.SetLocalValue(name, value);
        return value;
    }

    /// <summary>
    /// Creates an alias for an existing method on a class or module.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="context"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    internal static object AliasMethod(DynamicObject obj, Context context, IList<object> values)
    {
        if (obj is not DynamicClass target)
            throw new TypeError("alias_method can only be called on classes or modules");

        if (values.Count != 2)
            throw new ArgumentError($"wrong number of arguments (given {values.Count}, expected 2)");

        string newName = values[0] switch
        {
            Symbol symbol => symbol.Name,
            string text => text,
            _ => throw new TypeError("alias_method expects a symbol or string")
        };

        string oldName = values[1] switch
        {
            Symbol symbol => symbol.Name,
            string text => text,
            _ => throw new TypeError("alias_method expects a symbol or string")
        };

        target.AliasMethod(newName, oldName);
        return target;
    }

    /// <summary>
    /// Lists class variables on a class or module.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="context"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    internal static object ClassVariables(DynamicObject obj, Context context, IList<object> values)
    {
        if (obj is not DynamicClass target)
            throw new TypeError("class_variables can only be called on classes or modules");

        if (values.Count != 0)
            throw new ArgumentError($"wrong number of arguments (given {values.Count}, expected 0)");

        var result = new DynamicArray();
        foreach (var key in target.GetValues().Keys)
            result.Add(new Symbol("@@" + key));

        return result;
    }

    /// <summary>
    /// Sends a method call to an object by name.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="context"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    internal static object Send(DynamicObject obj, Context context, IList<object> values)
        => Send(obj, context, values, null);

    /// <summary>
    /// Sends a method call to an object by name with an optional block.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="context"></param>
    /// <param name="values"></param>
    /// <param name="block"></param>
    /// <returns></returns>
    internal static object Send(DynamicObject obj, Context context, IList<object> values, IFunction? block)
    {
        if (values.Count < 1)
            throw new ArgumentError($"wrong number of arguments (given {values.Count}, expected 1)");

        string name = values[0] switch
        {
            Symbol symbol => symbol.Name,
            string text => text,
            _ => throw new TypeError("send expects a symbol or string")
        };

        var args = values.Count > 1 ? values.Skip(1).ToList() : [];

        var method = obj.GetMethod(name);
        if (method == null)
        {
            if (TryInvokeMethodMissing(obj, context, name, args, block, out var methodMissingResult))
                return methodMissingResult;

            throw new NoMethodError(name);
        }

        return ApplyWithOptionalBlock(obj, context, method, args, block);
    }

    /// <summary>
    /// Defines a method on a class or module using a Proc or block.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="context"></param>
    /// <param name="values"></param>
    /// <param name="block"></param>
    /// <returns></returns>
    internal static object DefineMethod(DynamicObject obj, Context context, IList<object> values, IFunction? block)
    {
        if (obj is not DynamicClass target)
            throw new TypeError("define_method can only be called on classes or modules");

        if (values.Count < 1 || values.Count > 2)
            throw new ArgumentError($"wrong number of arguments (given {values.Count}, expected 1..2)");

        string name = values[0] switch
        {
            Symbol symbol => symbol.Name,
            string text => text,
            _ => throw new TypeError("define_method expects a symbol or string")
        };

        if (values.Count == 2 && block != null)
            throw new ArgumentError("define_method accepts either a Proc argument or a block, not both");

        DefinedFunction function = values.Count == 2
            ? BuildDefinedFunctionFromProc(values[1], context)
            : BuildDefinedFunctionFromBlock(block, context);

        target.SetInstanceMethod(name, function);
        return target;
    }

    /// <summary>
    /// Builds the defined function from proc.
    /// </summary>
    /// <param name="procValue">The proc object.</param>
    /// <param name="context">The context in which the function is defined.</param>
    /// <returns>A DefinedFunction instance.</returns>
    private static DefinedFunction BuildDefinedFunctionFromProc(object procValue, Context context)
    {
        if (procValue is not Proc proc)
            throw new TypeError("define_method expects a Proc for the second argument");

        if (proc.Block != null)
            return BuildDefinedFunctionFromBlock(proc.Block, proc.Block.ClosureContext);

        if (proc.Function is BlockAdapter adapter)
            return BuildDefinedFunctionFromBlock(adapter.Block, adapter.Block.ClosureContext);

        if (proc.Function is BlockFunction blockFunction)
            return BuildDefinedFunctionFromBlock(blockFunction.BlockExpression, context);

        throw new TypeError("define_method expects a Proc backed by a block");
    }

    /// <summary>
    /// Builds the defined function from block.
    /// </summary>
    /// <param name="block">The block function.</param>
    /// <param name="context">The context in which the function is defined.</param>
    /// <returns>A DefinedFunction instance.</returns>
    private static DefinedFunction BuildDefinedFunctionFromBlock(IFunction? block, Context context)
    {
        if (block == null)
            throw new ArgumentError("define_method requires a Proc or block");

        if (block is BlockFunction blockFunction)
            return BuildDefinedFunctionFromBlock(blockFunction.BlockExpression, context);

        if (block is BlockAdapter adapter)
            return BuildDefinedFunctionFromBlock(adapter.Block, context);

        throw new TypeError("define_method expects a block");
    }

    /// <summary>
    /// Builds the defined function from block expression.
    /// </summary>
    /// <param name="blockExpression">The block expression.</param>
    /// <param name="context">The context in which the function is defined.</param>
    /// <returns>A DefinedFunction instance.</returns>
    private static DefinedFunction BuildDefinedFunctionFromBlock(Embers.Expressions.BlockExpression blockExpression, Context context)
    {
        var parameters = blockExpression.Parameters ?? [];
        return new DefinedFunction(blockExpression.Body, parameters, null, null, context);
    }

    /// <summary>
    /// Builds the defined function from block.
    /// </summary>
    /// <param name="block">The block function.</param>
    /// <param name="context">The context in which the function is defined.</param>
    /// <returns>A DefinedFunction instance.</returns>
    private static DefinedFunction BuildDefinedFunctionFromBlock(Block block, Context context)
    {
        var parameters = block.ArgumentNames ?? [];
        return new DefinedFunction(block.Body, parameters, null, null, context);
    }

    /// <summary>
    /// Tries to invoke method missing.
    /// </summary>
    /// <param name="obj">The dynamic object.</param>
    /// <param name="context">The context.</param>
    /// <param name="missingName">The name of the missing method.</param>
    /// <param name="args">The arguments.</param>
    /// <param name="block">The block function.</param>
    /// <param name="result">The result of the invocation.</param>
    /// <returns>True if the method missing was invoked; otherwise, false.</returns>
    internal static bool TryInvokeMethodMissing(DynamicObject obj, Context context, string missingName, IList<object> args, IFunction? block, out object? result)
    {
        result = null;

        if (missingName == "method_missing")
            return false;

        var methodMissing = obj.GetMethod("method_missing");
        if (methodMissing == null)
            return false;

        var guard = MethodMissingGuards.GetValue(context.RootContext, _ => new HashSet<string>(StringComparer.Ordinal));
        if (!guard.Add(missingName))
            throw new NoMethodError(missingName);

        try
        {
            var values = new List<object> { new Symbol(missingName) };
            if (args.Count > 0)
                values.AddRange(args);

            result = ApplyWithOptionalBlock(obj, context, methodMissing, values, block);
            return true;
        }
        finally
        {
            guard.Remove(missingName);
        }
    }

    /// <summary>
    /// Applies the method with an optional block.
    /// </summary>
    /// <param name="obj">The dynamic object.</param>
    /// <param name="context">The context.</param>
    /// <param name="method">The method to apply.</param>
    /// <param name="values">The values to pass to the method.</param>
    /// <param name="block">The optional block function.</param>
    /// <returns>The result of the method application.</returns>
    private static object ApplyWithOptionalBlock(DynamicObject obj, Context context, IFunction method, IList<object> values, IFunction? block)
    {
        if (block != null && method is ICallableWithBlock callableWithBlock)
            return callableWithBlock.ApplyWithBlock(obj, context, values, block);

        if (block != null)
        {
            var applyWithBlockMethod = method.GetType().GetMethod("ApplyWithBlock");
            if (applyWithBlockMethod != null)
                return applyWithBlockMethod.Invoke(method, [obj, context, values, block]);
        }

        return method.Apply(obj, context, values);
    }

    /// <summary>
    /// Includes a module into a class or module.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="context"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    internal static object IncludeModule(DynamicObject obj, Context context, IList<object> values)
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

    /// <summary>
    /// Structures the new.
    /// </summary>
    /// <param name="obj">The object.</param>
    /// <param name="context">The context.</param>
    /// <param name="values">The values.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentError"></exception>
    internal static object StructNew(DynamicObject obj, Context context, IList<object> values)
    {
        if (values == null || values.Count == 0)
            throw new ArgumentError("Struct.new expects at least one member");

        int startIndex = 0;
        string? structName = null;

        if (values[0] is string nameText && Predicates.IsConstantName(nameText))
        {
            structName = nameText;
            startIndex = 1;
        }
        else if (values[0] is Symbol symbolName && Predicates.IsConstantName(symbolName.Name))
        {
            structName = symbolName.Name;
            startIndex = 1;
        }

        if (startIndex >= values.Count)
            throw new ArgumentError("Struct.new expects at least one member");

        var memberNames = new List<string>();
        for (int i = startIndex; i < values.Count; i++)
        {
            string memberName = values[i] switch
            {
                Symbol symbol => symbol.Name,
                string text => text,
                _ => throw new TypeError("Struct member names must be symbols or strings")
            };

            if (string.IsNullOrWhiteSpace(memberName))
                throw new ArgumentError("Struct member name cannot be empty");

            if (memberNames.Contains(memberName))
                throw new ArgumentError($"duplicate member name '{memberName}'");

            memberNames.Add(memberName);
        }

        var classclass = (DynamicClass)context.RootContext.GetLocalValue("Class");
        var objectclass = (DynamicClass)context.RootContext.GetLocalValue("Object");
        var parent = context.Module;
        var name = structName ?? $"Struct{++anonymousStructIndex}";
        var structClass = new DynamicClass(classclass, name, objectclass, parent);
        structClass.SetValue("__struct_members__", memberNames);

        foreach (var member in memberNames)
        {
            structClass.SetInstanceMethod(member, new StructMemberGetFunction(member));
            structClass.SetInstanceMethod(member + "=", new StructMemberSetFunction(member));
        }

        structClass.SetInstanceMethod("initialize", new StructInitializeFunction(memberNames));
        structClass.SingletonClass.SetInstanceMethod("members", new StructMembersFunction(memberNames));

        if (structName != null)
        {
            if (parent == null)
                context.RootContext.SetLocalValue(structName, structClass);
            else
                parent.Constants.SetLocalValue(structName, structClass);
        }

        return structClass;
    }

}

