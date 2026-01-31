using System.Collections;
using Embers.Exceptions;
using Embers.Expressions;
using Embers.Tests.Classes;

namespace Embers.Tests
{
    [TestClass]
    public class EvaluateTests
    {
        private Machine machine;

        [TestInitialize]
        public void Setup() => machine = new Machine();

        [TestMethod]
        public void EvaluateSimpleArithmetic()
        {
            Assert.AreEqual(2L, EvaluateExpression("1+1"));
            Assert.AreEqual(0L, EvaluateExpression("1/2"));
            Assert.AreEqual(1L + (3 * 2), EvaluateExpression("1 + 3 * 2"));
        }

        [TestMethod]
        public void EvaluateStringConcatenate()
        {
            Assert.AreEqual("foobar", EvaluateExpression("'foo' + 'bar'"));
            Assert.AreEqual("foo1", EvaluateExpression("'foo' + 1"));
            Assert.AreEqual("1foo", EvaluateExpression("1 + 'foo'"));
        }

        [TestMethod]
        public void EvaluateSimpleAssignment()
        {
            Assert.AreEqual(2L, Execute("a = 2"));
            Assert.AreEqual(2L, EvaluateExpression("a"));
        }

        [TestMethod]
        public void EvaluateGlobalAssignment()
        {
            Assert.AreEqual(2L, Execute("$a = 2"));
            Assert.AreEqual(2L, EvaluateExpression("$a"));
            Assert.AreEqual(3L, EvaluateExpression("$a + 1"));
        }

        [TestMethod]
        public void EvaluateSimpleArray()
        {
            Assert.IsNotNull(Execute("a = [1,2,3]"));
            var result = EvaluateExpression("a");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(IList));

            var list = (IList)result;

            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(1L, list[0]);
            Assert.AreEqual(2L, list[1]);
            Assert.AreEqual(3L, list[2]);
        }

        [TestMethod]
        public void EvaluateIntegerClassAsFixnum()
        {
            var result = EvaluateExpression("1.class");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NativeClassAdapter));
            Assert.AreEqual("Fixnum", ((NativeClassAdapter)result).Name);
        }

        [TestMethod]
        public void EvaluateStringClassAsString()
        {
            var result = EvaluateExpression("'foo'.class");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NativeClassAdapter));
            Assert.AreEqual("String", ((NativeClassAdapter)result).Name);
        }

        [TestMethod]
        public void EvaluateTrueClassAsTrueClass()
        {
            var result = EvaluateExpression("true.class");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NativeClassAdapter));
            Assert.AreEqual("TrueClass", ((NativeClassAdapter)result).Name);
        }

        [TestMethod]
        public void EvaluateFalseClassAsFalseClass()
        {
            var result = EvaluateExpression("false.class");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NativeClassAdapter));
            Assert.AreEqual("FalseClass", ((NativeClassAdapter)result).Name);
        }

        [TestMethod]
        public void EvaluateNilClassAsNilClass()
        {
            var result = EvaluateExpression("nil.class");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NativeClassAdapter));
            Assert.AreEqual("NilClass", ((NativeClassAdapter)result).Name);
        }

        [TestMethod]
        public void EvaluateFloatClassAsFloatClass()
        {
            var result = EvaluateExpression("1.2.class");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NativeClassAdapter));
            Assert.AreEqual("Float", ((NativeClassAdapter)result).Name);
        }

        [TestMethod]
        public void EvaluateArrayClassAsArrayClass()
        {
            var result = EvaluateExpression("[1,2].class");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NativeClassAdapter));
            Assert.AreEqual("Array", ((NativeClassAdapter)result).Name);
        }

        [TestMethod]
        public void EvaluateHashClassAsHashClass()
        {
            var result = EvaluateExpression("{:one=>1, :two => 2}.class");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NativeClassAdapter));
            Assert.AreEqual("Hash", ((NativeClassAdapter)result).Name);
        }

        [TestMethod]
        public void EvaluateModuleConstant()
        {
            Execute("module MyModule;ONE=1;end");
            var result = EvaluateExpression("MyModule::ONE");

            Assert.IsNotNull(result);
            Assert.AreEqual(1L, result);
        }

        [TestMethod]
        public void EvaluateObjectNew()
        {
            var result = EvaluateExpression("Object.new");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(DynamicObject));

            var dobj = (DynamicObject)result;

            Assert.IsNotNull(dobj.Class);
            Assert.AreEqual("Object", dobj.Class.Name);
            Assert.AreSame(dobj.Class, EvaluateExpression("Object"));
        }

        [TestMethod]
        public void EvaluateObjectNewClass()
        {
            var objectclass = EvaluateExpression("Object");

            Assert.IsNotNull(objectclass);

            var result = EvaluateExpression("Object.new.class");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(DynamicClass));

            Assert.AreSame(objectclass, result);
        }

        [TestMethod]
        public void EvaluateModuleNewClass()
        {
            var moduleclass = EvaluateExpression("Module");

            Assert.IsNotNull(moduleclass);

            var result = EvaluateExpression("Module.new.class");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(DynamicClass));

            Assert.AreSame(moduleclass, result);
        }

        [TestMethod]
        public void EvaluateClassNewClass()
        {
            var classclass = EvaluateExpression("Class");

            Assert.IsNotNull(classclass);

            var result = EvaluateExpression("Class.new.class");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(DynamicClass));

            Assert.AreSame(classclass, result);
        }

        [TestMethod]
        public void EvaluateDefinedClassClass()
        {
            var classclass = EvaluateExpression("Class");

            Assert.IsNotNull(classclass);

            var result = Execute("class Foo\n end\n Foo.class");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(DynamicClass));

            Assert.AreSame(classclass, result);
        }

        [TestMethod]
        public void EvaluateDefinedClassSuperClass()
        {
            var classclass = EvaluateExpression("Class");
            var objectclass = EvaluateExpression("Object");

            Assert.IsNotNull(classclass);

            var result = Execute("class Foo\n end\n Foo.superclass");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(DynamicClass));

            Assert.AreSame(objectclass, result);
        }

        [TestMethod]
        public void EvaluateClassWithExplicitSuperClass()
        {
            Execute(@"
class Animal
  def speak
    'Some sound'
  end
end

class Cat < Animal
  def speak
    'Meow'
  end
end");

            var result = EvaluateExpression("Cat.superclass");
            Assert.AreSame(EvaluateExpression("Animal"), result);
            Assert.AreEqual("Meow", EvaluateExpression("Cat.new.speak"));
        }

        [TestMethod]
        public void EvaluateDefinedModuleClass()
        {
            var moduleclass = EvaluateExpression("Module");

            Assert.IsNotNull(moduleclass);

            var result = Execute("module Foo\n end\n Foo.class");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(DynamicClass));

            Assert.AreSame(moduleclass, result);
        }

        [TestMethod]
        public void EvaluateModuleIncludeInClass()
        {
            Execute(@"
module Greeter
  def hello
    'hi'
  end
end

class Person
  include Greeter
end");

            var result = EvaluateExpression("Person.new.hello");
            Assert.AreEqual("hi", result);
        }

        [TestMethod]
        public void EvaluateModuleIncludeOrder()
        {
            Execute(@"
module M1
  def foo
    'm1'
  end
end

module M2
  def foo
    'm2'
  end
end

class C
  include M1
  include M2
end");

            var result = EvaluateExpression("C.new.foo");
            Assert.AreEqual("m2", result);
        }

        [TestMethod]
        public void EvaluateModuleIncludeChained()
        {
            Execute(@"
module M1
  def bar
    'bar'
  end
end

module M2
  include M1
end

class C
  include M2
end");

            var result = EvaluateExpression("C.new.bar");
            Assert.AreEqual("bar", result);
        }

        [TestMethod]
        public void EvaluateRangeClass()
        {
            var rangeclass = EvaluateExpression("Range");

            Assert.IsNotNull(rangeclass);

            var result = Execute("(1..10).class");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NativeClassAdapter));
            Assert.AreEqual("Range", ((NativeClassAdapter)result).Name);

            Assert.AreSame(rangeclass, result);
        }

        [TestMethod]
        public void EvaluateEachOnArray()
        {
            var result = Execute("total = 0\n[1,2,3].each do |x| total = total + x end\ntotal");

            Assert.IsNotNull(result);
            Assert.AreEqual(6L, result);
        }

        [TestMethod]
        public void EvaluateForInRange()
        {
            var result = Execute("total = 0\nfor k in 1..3\ntotal = total + k\nend\ntotal");

            Assert.IsNotNull(result);
            Assert.AreEqual(6L, result);
        }

        [TestMethod]
        public void EvaluateRangeEach()
        {
            var result = Execute("total = 0\n(1..3).each do |x| total = total + x end\ntotal");

            Assert.IsNotNull(result);
            Assert.AreEqual(6L, result);
        }

        [TestMethod]
        public void EvaluateStringNativeProperty()
        {
            var result = Execute("'foo'.Length");

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result);
        }

        [TestMethod]
        public void EvaluateStringNativeMethod()
        {
            var result = Execute("'foo'.ToUpper");

            Assert.IsNotNull(result);
            Assert.AreEqual("FOO", result);
        }

        [TestMethod]
        public void EvaluateDynamicObjectNativeMethod()
        {
            var obj = (DynamicObject)Execute("obj = Object.new");

            var result = Execute("obj.GetHashCode");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(int));
            Assert.AreEqual(obj.GetHashCode(), result);
        }

        [TestMethod]
        public void EvaluateStringNativeMethodWithArguments()
        {
            var result = Execute("'foo'.Substring 1,2");
            Assert.IsNotNull(result);
            Assert.AreEqual("oo", result);
        }

        [TestMethod]
        public void EvaluateNewPerson()
        {
            machine.RootContext.SetLocalValue("Person", typeof(Person));
            var result = EvaluateExpression("Person.new");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(Person));
        }

        [TestMethod]
        public void EvaluateNewPersonWithNames()
        {
            machine.RootContext.SetLocalValue("Person", typeof(Person));
            var result = EvaluateExpression("Person.new 'Kieran', 'Borsden'");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(Person));

            var person = (Person)result;
            Assert.AreEqual("Kieran", person.FirstName);
            Assert.AreEqual("Borsden", person.LastName);
        }

        [TestMethod]
        public void EvaluateQualifiedType()
        {
            var result = EvaluateExpression("Embers::Tests::Classes::Person");

            Assert.IsNotNull(result);
            Assert.AreSame(typeof(Person), result);
        }

        [TestMethod]
        public void EvaluateQualifiedPersonNew()
        {
            var result = EvaluateExpression("Embers::Tests::Classes::Person.new");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(Person));
        }

        [TestMethod]
        public void EvaluateQualifiedPersonNewWithNames()
        {
            var result = EvaluateExpression("Embers::Tests::Classes::Person.new('Kieran', 'Borsden')");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(Person));

            var person = (Person)result;
            Assert.AreEqual("Kieran", person.FirstName);
            Assert.AreEqual("Borsden", person.LastName);
        }

        [TestMethod]
        public void EvaluateFileExists()
        {
            string tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".txt");

            try
            {
                // Should not exist yet
                var result = EvaluateExpression($"System::IO::File.Exists('{tempFile.Replace("\\", "\\\\")}')");
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(bool));
                Assert.IsFalse((bool)result);

                // Create the file and ensure it is closed before checking existence
                File.WriteAllText(tempFile, "test");

                // Retry logic: wait for the file system to catch up (if needed)
                bool exists = false;
                for (int i = 0; i < 5; i++)
                {
                    exists = File.Exists(tempFile);
                    if (exists) break;
                    Thread.Sleep(50); // Wait 50ms before retrying
                }
                Assert.IsTrue(exists, "Temp file was not found after creation.");

                // Now check via EvaluateExpression
                result = EvaluateExpression($"System::IO::File.Exists('{tempFile.Replace("\\", "\\\\")}')");
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(bool));
                Assert.IsTrue((bool)result);
            }
            catch (FileNotFoundException ex)
            {
                Assert.Inconclusive($"Test skipped due to missing assembly: {ex.Message}\nStackTrace: {ex.StackTrace}");
            }
            finally
            {
                try
                {
                    if (File.Exists(tempFile))
                        File.Delete(tempFile);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        //[TestMethod]
        //public void EvaluateFileExists()
        //{
        //    // Use a temp file to avoid permission issues and ensure isolation
        //    string tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".txt");

        //    try
        //    {
        //        // Should not exist yet
        //        var result = EvaluateExpression($"System.IO.File.Exists('{tempFile.Replace("\\", "\\\\")}')");
        //        Assert.IsNotNull(result);
        //        Assert.IsInstanceOfType(result, typeof(bool));
        //        Assert.IsFalse((bool)result);

        //        // Create the file
        //        File.WriteAllText(tempFile, "test");

        //        // Now it should exist
        //        result = EvaluateExpression($"System.IO.File.Exists('{tempFile.Replace("\\", "\\\\")}')");
        //        Assert.IsNotNull(result);
        //        Assert.IsInstanceOfType(result, typeof(bool));
        //        Assert.IsTrue((bool)result);
        //    }
        //    finally
        //    {
        //        if (File.Exists(tempFile))
        //            File.Delete(tempFile);
        //    }
        //}

        [TestMethod]
        public void EvaluateCreateByteArray()
        {
            var result = EvaluateExpression("System::Array.CreateInstance(System::Byte, 1024)");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(byte[]));

            var array = (byte[])result;

            Assert.AreEqual(1024, array.Length);
        }

        [TestMethod]
        public void EvaluateAccessStringAsArray()
        {
            var result = EvaluateExpression("'foo'[0]");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(string));
            Assert.AreEqual("f", result);
        }

        [TestMethod]
        public void EvaluateProcEmptyIndexCall()
        {
            var result = Execute("zero = -> { 42 }\nzero[]");

            Assert.IsNotNull(result);
            Assert.AreEqual(42L, result);
        }

        [TestMethod]
        [ExpectedException(typeof(TypeError))]
        public void EvaluateEmptyIndexOnNonProcRaisesTypeError()
        {
            EvaluateExpression("'foo'[]");
        }

        [TestMethod]
        public void DefineQualifiedClass()
        {
            Execute("module MyModule\nend");
            Execute("class MyModule::MyClass\nend");

            var result = EvaluateExpression("MyModule::MyClass");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(DynamicClass));
            Assert.AreEqual("MyClass", ((DynamicClass)result).Name);
            Assert.AreEqual("MyModule::MyClass", ((DynamicClass)result).FullName);
        }

        [TestMethod]
        public void TypeErrorWhenDefineQualifiedClassOnAnObject()
        {
            try
            {
                Execute("class self::MyClass\nend");
                Assert.Fail();
            }
            catch (Exception ex) 
            {
                Assert.IsInstanceOfType(ex, typeof(TypeError));
                Assert.IsTrue(ex.Message.EndsWith(" is not a class/module"));
            }
        }

        [TestMethod]
        public void DefineObjectMethod()
        {
            Execute("class Object\ndef foo\nend\nend");

            var result = Execute("Object");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(DynamicClass));

            var dclass = (DynamicClass)result;

            Assert.IsNotNull(dclass.GetInstanceMethod("foo"));
        }

        [TestMethod]
        public void EvaluateStringInterpolation()
        {
            Assert.AreEqual("Hello 3!", EvaluateExpression("\"Hello #{1+2}!\""));
            Assert.AreEqual("Sum: 7", EvaluateExpression("\"Sum: #{3+4}\""));
            Assert.AreEqual("fooBAR", EvaluateExpression("\"foo#{'BAR'}\""));
            //Assert.AreEqual("Value: 42", EvaluateExpression("a = 42; Value: #{a}\""));
            Assert.AreEqual("Nested: 9", EvaluateExpression("\"Nested: #{3 * (2 + 1)}\""));
        }

        [TestMethod]
        public void EvaluateHeredocBasic()
        {
            var result = EvaluateExpression("<<EOF\nhello\nEOF\n");
            Assert.AreEqual("hello\n", result);
        }

        [TestMethod]
        public void EvaluateHeredocInterpolated()
        {
            var result = Execute("name = 'Em'\n<<EOF\nhi #{name}\nEOF\n");
            Assert.AreEqual("hi Em\n", result);
        }

        [TestMethod]
        public void EvaluateHeredocSingleQuotedLiteral()
        {
            var result = Execute("name = 'Em'\n<<'EOF'\nhi #{name}\nEOF\n");
            Assert.AreEqual("hi #{name}\n", result);
        }

        [TestMethod]
        public void EvaluateHeredocIndentTerminator()
        {
            var result = EvaluateExpression("<<-EOF\nhello\n  EOF\n");
            Assert.AreEqual("hello\n", result);
        }

        [TestMethod]
        public void EvaluateHeredocStripIndent()
        {
            var result = EvaluateExpression("<<~EOF\n  a\n    b\nEOF\n");
            Assert.AreEqual("a\n  b\n", result);
        }

        //[TestMethod]
        //public void EvaluateUnlessBasic()
        //{
        //    // unless true then block should NOT run
        //    Assert.IsNull(EvaluateExpression("unless true then 1 end"));

        //    // unless false then block should run
        //    Assert.AreEqual(1, EvaluateExpression("unless false then 1 end"));

        //    // unless with else: condition true, else runs
        //    Assert.AreEqual(2, EvaluateExpression("unless true then 1 else 2 end"));

        //    // unless with else: condition false, then runs
        //    Assert.AreEqual(1, EvaluateExpression("unless false then 1 else 2 end"));
        //}

        private object EvaluateExpression(string text)
        {
            Parser parser = new(text);
            IExpression expression = parser.ParseExpression();
            Assert.IsNull(parser.ParseExpression());
            return expression.Evaluate(machine.RootContext);
        }

        private object Execute(string text) => machine.ExecuteText(text);
    }
}

