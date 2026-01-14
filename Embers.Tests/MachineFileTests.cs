using Embers.Language;
using Embers.StdLib;

namespace Embers.Tests
{
    [TestClass]
    [DeploymentItem("MachineFiles", "MachineFiles")]
    public class MachineFileTests
    {
        private Machine machine;

        [TestInitialize]
        public void Setup() => machine = new Machine();

        [TestMethod]
        public void ExecuteSimpleAssignFile()
        {
            Assert.AreEqual(1L, machine.ExecuteFile("MachineFiles\\SimpleAssign.rb"));
            Assert.AreEqual(1L, machine.RootContext.GetValue("a"));
        }

        [TestMethod]
        public void ExecuteSimpleAssignsFile()
        {
            Assert.AreEqual(2L, machine.ExecuteFile("MachineFiles\\SimpleAssigns.rb"));
            Assert.AreEqual(1L, machine.RootContext.GetValue("a"));
            Assert.AreEqual(2L, machine.RootContext.GetValue("b"));
        }

        [TestMethod]
        public void ExecuteSimplePutsFile()
        {
            StringWriter writer = new();
            machine.RootContext.Self.Class.SetInstanceMethod("puts", new PutsFunction(writer));
            Assert.AreEqual(null, machine.ExecuteFile("MachineFiles\\SimplePuts.rb"));
            Assert.AreEqual("hello\r\n", writer.ToString());
        }

        [TestMethod]
        public void ExecuteSimpleDefFile()
        {
            StringWriter writer = new();
            machine.RootContext.Self.Class.SetInstanceMethod("puts", new PutsFunction(writer));
            Assert.AreEqual(null, machine.ExecuteFile("MachineFiles\\SimpleDef.rb"));
            Assert.AreEqual("1\r\n", writer.ToString());
        }

        [TestMethod]
        public void ExecuteSimpleClassFile()
        {
            StringWriter writer = new();
            machine.RootContext.Self.Class.SetInstanceMethod("puts", new PutsFunction(writer));
            Assert.AreEqual(null, machine.ExecuteFile("MachineFiles\\SimpleClass.rb"));
            Assert.AreEqual("Hello\r\n", writer.ToString());

            var result = machine.RootContext.GetValue("Dog");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(DynamicClass));

            var dclass = (DynamicClass)result;

            Assert.AreEqual("Dog", dclass.Name);
        }

        [TestMethod]
        public void ExecuteNewInstanceFile()
        {
            var result = machine.ExecuteFile("MachineFiles\\NewInstance.rb");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(DynamicObject));

            var obj = (DynamicObject)result;

            Assert.AreEqual("Nero", obj.GetValue("name"));
            Assert.AreEqual(6L, obj.GetValue("age"));
            Assert.AreEqual("Dog", obj.Class.Name);
        }

        [TestMethod]
        public void ExecutePointFile()
        {
            machine.ExecuteFile("MachineFiles\\Point.rb");

            var result = machine.ExecuteText("Point");
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(DynamicClass));
        }

        [TestMethod]
        public void CreatePoint()
        {
            machine.ExecuteFile("MachineFiles\\Point.rb");

            var result = machine.ExecuteText("Point.new(10, 20)");
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(DynamicObject));

            var dobj = (DynamicObject)result;

            Assert.AreEqual(10L, dobj.GetValue("x"));
            Assert.AreEqual(20L, dobj.GetValue("y"));
        }

        [TestMethod]
        public void RequireFile()
        {
            Assert.IsTrue(machine.RequireFile("MachineFiles\\Point"));

            var result = machine.ExecuteText("Point");
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(DynamicClass));
        }

        [TestMethod]
        public void RequireLibraryFile()
        {
            Assert.IsTrue(machine.RequireFile("MachineFiles\\MyLib"));

            var result = machine.ExecuteText("MyLib::MyClass");
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(Type));
        }

        [TestMethod]
        public void RequireFileTwice()
        {
            Assert.IsTrue(machine.RequireFile("MachineFiles\\SimpleAssign"));
            Assert.IsFalse(machine.RequireFile("MachineFiles\\SimpleAssign.rb"));

            var result = machine.ExecuteText("a");
            Assert.IsNotNull(result);
            Assert.AreEqual(1L, result);
        }

        [TestMethod]
        public void RequireFileTwiceUsingFullPath()
        {
            string filename = Path.GetFullPath("MachineFiles\\SimpleAssign");
            string filename2 = Path.GetFullPath("MachineFiles\\SimpleAssign.rb");
            Assert.IsTrue(machine.RequireFile(filename));
            Assert.IsFalse(machine.RequireFile(filename2));

            var result = machine.ExecuteText("a");
            Assert.IsNotNull(result);
            Assert.AreEqual(1L, result);
        }

        [TestMethod]
        public void ExecuteRequireModules()
        {
            machine.ExecuteFile("MachineFiles\\RequireModules.rb");

            var result = machine.ExecuteText("Module1");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(DynamicClass));
            Assert.AreEqual("Module1", ((DynamicClass)result).Name);

            result = machine.ExecuteText("Module2");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(DynamicClass));
            Assert.AreEqual("Module2", ((DynamicClass)result).Name);
        }

        [TestMethod]
        public void ExecuteRepeatedModuleFile()
        {
            machine.ExecuteFile("MachineFiles\\RepeatedModule.rb");

            Assert.AreEqual(1L, machine.ExecuteText("MyModule::ONE"));
            Assert.AreEqual(2L, machine.ExecuteText("MyModule::TWO"));
        }

        [TestMethod]
        public void ExecuteModuleWithClassesFile()
        {
            machine.ExecuteFile("MachineFiles\\ModuleWithClasses.rb");

            var result1 = machine.ExecuteText("MyLisp::List");
            var result2 = machine.ExecuteText("MyLisp::Atom");

            Assert.IsNotNull(result1);
            Assert.IsInstanceOfType(result1, typeof(DynamicClass));
            Assert.AreEqual("List", ((DynamicClass)result1).Name);
            Assert.AreEqual("MyLisp::List", ((DynamicClass)result1).FullName);

            Assert.IsNotNull(result2);
            Assert.IsInstanceOfType(result2, typeof(DynamicClass));
            Assert.AreEqual("Atom", ((DynamicClass)result2).Name);
            Assert.AreEqual("MyLisp::Atom", ((DynamicClass)result2).FullName);
        }

        [TestMethod]
        public void ExecuteClassWithModuleFile()
        {
            machine.ExecuteFile("MachineFiles\\ClassWithModule.rb");

            var result1 = machine.ExecuteText("MyClass");
            var result2 = machine.ExecuteText("MyClass::MyModule");

            Assert.IsNotNull(result1);
            Assert.IsInstanceOfType(result1, typeof(DynamicClass));
            Assert.AreEqual("MyClass", ((DynamicClass)result1).Name);
            Assert.AreEqual("MyClass", ((DynamicClass)result1).FullName);

            Assert.IsNotNull(result2);
            Assert.IsInstanceOfType(result2, typeof(DynamicClass));
            Assert.AreEqual("MyModule", ((DynamicClass)result2).Name);
            Assert.AreEqual("MyClass::MyModule", ((DynamicClass)result2).FullName);
        }

        [TestMethod]
        public void ExecuteNestedModulesFile()
        {
            machine.ExecuteFile("MachineFiles\\NestedModules.rb");

            var result1 = machine.ExecuteText("MyModule");
            var result2 = machine.ExecuteText("MyModule::MySubmodule");
            var result3 = machine.ExecuteText("MyModule::MySubmodule::MySubmodule2");

            Assert.IsNotNull(result1);
            Assert.IsInstanceOfType(result1, typeof(DynamicClass));
            Assert.AreEqual("MyModule", ((DynamicClass)result1).Name);

            Assert.IsNotNull(result2);
            Assert.IsInstanceOfType(result2, typeof(DynamicClass));
            Assert.AreEqual("MySubmodule", ((DynamicClass)result2).Name);
            Assert.AreEqual("MyModule::MySubmodule", ((DynamicClass)result2).FullName);

            Assert.IsNotNull(result3);
            Assert.IsInstanceOfType(result3, typeof(DynamicClass));
            Assert.AreEqual("MySubmodule2", ((DynamicClass)result3).Name);
            Assert.AreEqual("MyModule::MySubmodule::MySubmodule2", ((DynamicClass)result3).FullName);
        }

        [TestMethod]
        public void ExecuteModuleWithSelfMethodFile()
        {
            machine.ExecuteFile("MachineFiles\\ModuleWithSelfMethod.rb");

            var result1 = machine.ExecuteText("MyModule");
            var result2 = machine.ExecuteText("MyModule.foo");

            Assert.IsNotNull(result1);
            Assert.IsInstanceOfType(result1, typeof(DynamicClass));
            Assert.AreEqual("MyModule", ((DynamicClass)result1).Name);
            Assert.AreSame(machine.ExecuteText("Module"), machine.ExecuteText("MyModule.class"));

            Assert.IsNotNull(result2);
            Assert.AreEqual(42L, result2);
        }

        [TestMethod]
        public void ExecuteClassWithSelfMethodFile()
        {
            machine.ExecuteFile("MachineFiles\\ClassWithSelfMethod.rb");

            var result1 = machine.ExecuteText("MyClass");
            var result2 = machine.ExecuteText("MyClass.foo");

            Assert.IsNotNull(result1);
            Assert.IsInstanceOfType(result1, typeof(DynamicClass));
            Assert.AreEqual("MyClass", ((DynamicClass)result1).Name);
            Assert.AreSame(machine.ExecuteText("Class"), machine.ExecuteText("MyClass.class"));

            Assert.IsNotNull(result2);
            Assert.AreEqual(42L, result2);
        }

        [TestMethod]
        public void Execute_WithFilePath_ExecutesFile()
        {
            Assert.AreEqual(1L, machine.Execute("MachineFiles\\SimpleAssign.rb"));
            Assert.AreEqual(1L, machine.RootContext.GetValue("a"));
        }

        [TestMethod]
        public void Execute_WithCode_ExecutesText()
        {
            var result = machine.Execute("b = 5; b * 2");
            Assert.AreEqual(10L, result);
            Assert.AreEqual(5L, machine.RootContext.GetValue("b"));
        }

        [TestMethod]
        public void Execute_WithNonExistentPath_ExecutesAsText()
        {
            // If the path doesn't look like a file, it should execute as text
            var result = machine.Execute("c = 100");
            Assert.AreEqual(100L, result);
            Assert.AreEqual(100L, machine.RootContext.GetValue("c"));
        }

        [TestMethod]
        public void Execute_WithNonExistentFilePath_ThrowsFileNotFoundException() =>
            // If it looks like a file path but doesn't exist, throw FileNotFoundException
            Assert.ThrowsException<FileNotFoundException>(() => machine.Execute("path/to/nonexistent.rb"));

        [TestMethod]
        public void Execute_WithNonExistentFilePathBackslash_ThrowsFileNotFoundException() =>
            // Test with Windows-style path separator
            Assert.ThrowsException<FileNotFoundException>(() => machine.Execute("path\\to\\nonexistent.rb"));
    }
}
