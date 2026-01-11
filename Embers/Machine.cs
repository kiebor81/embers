using System.Reflection;
using Embers.Compiler;
using Embers.Functions;
using Embers.Language;
using Embers.Security;

namespace Embers
{

    /// <summary>
    /// Machine is the main entry point for the Embers language.
    /// It holds the root context and provides methods to execute code.
    /// The machine is responsible for managing the execution environment,
    /// One can think of it as the runtime for the Embers language / representative of a ruby vm.
    /// </summary>
    public class Machine
    {
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

            moduleclass.SetInstanceMethod("superclass", new LambdaFunction(GetSuperClass));
            moduleclass.SetInstanceMethod("name", new LambdaFunction(GetName));

            classclass.SetInstanceMethod("new", new LambdaFunction(NewInstance));

            rootcontext.SetLocalValue("Fixnum", new FixnumClass(this));
            rootcontext.SetLocalValue("Float", new FloatClass(this));
            rootcontext.SetLocalValue("String", new StringClass(this));
            rootcontext.SetLocalValue("Symbol", new SymbolClass(this));
            rootcontext.SetLocalValue("NilClass", new NilClass(this));
            rootcontext.SetLocalValue("FalseClass", new FalseClass(this));
            rootcontext.SetLocalValue("TrueClass", new TrueClass(this));
            rootcontext.SetLocalValue("Array", new ArrayClass(this));
            rootcontext.SetLocalValue("Hash", new HashClass(this));
            rootcontext.SetLocalValue("Range", new RangeClass(this));
            rootcontext.SetLocalValue("DateTime", new DateTimeClass(this));
            rootcontext.SetLocalValue("JSON", new JsonClass(this));

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
        /// Executes the text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public object ExecuteText(string text)
        {
            Parser parser = new(text);
            object result = null;

            for (var command = parser.ParseCommand(); command != null; command = parser.ParseCommand())
                result = command.Evaluate(rootcontext);

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
                return ExecuteText(System.IO.File.ReadAllText(filename));
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
                        if (File.Exists(newfilename + ".rb"))
                            newfilename += ".rb";
                        else if (File.Exists(newfilename + ".dll"))
                            newfilename += ".dll";

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
                    if (File.Exists(newfilename + ".rb"))
                        newfilename += ".rb";
                    else if (File.Exists(newfilename + ".dll"))
                        newfilename += ".dll";

                filename = newfilename;
            }

            if (required.Contains(filename))
                return false;

            if (filename.EndsWith(".dll"))
            {
                Assembly.LoadFrom(filename);
                return true;
            }

            ExecuteFile(filename);
            required.Add(filename);

            return true;
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

            for (var command = parser.ParseCommand(); command != null; command = parser.ParseCommand())
                result = command.Evaluate(rootcontext);

            return result;
        }

        /// <summary>
        /// Sets the type access policy.
        /// Allowed entries are a list of full type names that are allowed to be accessed.
        /// Provide allowed entries as a list of strings where final character '.' implies a namespace.
        /// </summary>
        /// <param name="allowedEntries">The allowed entries.</param>
        public void SetTypeAccessPolicy(IEnumerable<string> allowedEntries, SecurityMode mode = SecurityMode.WhitelistOnly)
        {
            TypeAccessPolicy.SetPolicy(allowedEntries, mode);
        }

        /// <summary>
        /// Allows the type.
        /// </summary>
        /// <param name="fullTypeName">Full name of the type.</param>
        public void AllowType(string fullTypeName)
        {
            Security.TypeAccessPolicy.AddType(fullTypeName);
        }

        /// <summary>
        /// Allows the namespace.
        /// </summary>
        /// <param name="prefix">The prefix.</param>
        public void AllowNamespace(string prefix)
        {
            Security.TypeAccessPolicy.AddNamespace(prefix);
        }

        /// <summary>
        /// Clears the security policy.
        /// </summary>
        public void ClearSecurityPolicy()
        {
            Security.TypeAccessPolicy.Clear();
        }

        private static object NewInstance(DynamicObject obj, Context context, IList<object> values)
        {
            var newobj = ((DynamicClass)obj).CreateInstance();

            var initialize = newobj.GetMethod("initialize");

            initialize?.Apply(newobj, context, values);

            return newobj;
        }

        private static object GetName(DynamicObject obj, Context context, IList<object> values)
        {
            return ((DynamicClass)obj).Name;
        }

        private static object GetSuperClass(DynamicObject obj, Context context, IList<object> values)
        {
            return ((DynamicClass)obj).SuperClass;
        }

        private static object GetClass(DynamicObject obj, Context context, IList<object> values)
        {
            return obj.Class;
        }

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

    }
}
