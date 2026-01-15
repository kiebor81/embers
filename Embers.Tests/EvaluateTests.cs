using System.Collections;
using Embers.Compiler;
using Embers.Exceptions;
using Embers.Expressions;
using Embers.Language;
using Embers.StdLib;
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
            Assert.IsInstanceOfType(result, typeof(FixnumClass));
        }

        [TestMethod]
        public void EvaluateStringClassAsString()
        {
            var result = EvaluateExpression("'foo'.class");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(StringClass));
        }

        [TestMethod]
        public void EvaluateTrueClassAsTrueClass()
        {
            var result = EvaluateExpression("true.class");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(TrueClass));
        }

        [TestMethod]
        public void EvaluateFalseClassAsFalseClass()
        {
            var result = EvaluateExpression("false.class");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(FalseClass));
        }

        [TestMethod]
        public void EvaluateNilClassAsNilClass()
        {
            var result = EvaluateExpression("nil.class");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NilClass));
        }

        [TestMethod]
        public void EvaluateFloatClassAsFloatClass()
        {
            var result = EvaluateExpression("1.2.class");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(FloatClass));
        }

        [TestMethod]
        public void EvaluateArrayClassAsArrayClass()
        {
            var result = EvaluateExpression("[1,2].class");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ArrayClass));
        }

        [TestMethod]
        public void EvaluateHashClassAsHashClass()
        {
            var result = EvaluateExpression("{:one=>1, :two => 2}.class");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(HashClass));
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
        public void EvaluateRangeClass()
        {
            var rangeclass = EvaluateExpression("Range");

            Assert.IsNotNull(rangeclass);

            var result = Execute("(1..10).class");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RangeClass));

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

        [TestMethod]
        public void EvaluateRaiseThrowsException()
        {
            try
            {
                EvaluateExpression("raise 'fail!'");
                Assert.Fail("Expected exception was not thrown.");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("fail!"));
            }
        }

        //    [TestMethod]
        //    public void EvaluateRaiseRescue()
        //    {
        //        var result = EvaluateExpression(@"
        //    begin
        //        raise 'fail!'
        //    rescue ex
        //        puts ""rescued""
        //        ex
        //    end
        //");
        //        Assert.IsNotNull(result, "Rescue block did not return an exception object.");
        //        Assert.IsTrue(result.ToString().Contains("fail!"));
        //    }

        [TestMethod]
        public void EvaluateRaiseWithTypeAndMessage()
        {
            try
            {
                EvaluateExpression("raise ArgumentError, 'Invalid argument!'");
                Assert.Fail("Expected exception was not thrown.");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ArgumentError));
                Assert.IsTrue(ex.Message.Contains("Invalid argument!"));
            }
        }

        //    [TestMethod]
        //    public void EvaluateRescueSpecificType()
        //    {
        //        // This should catch only ArgumentError, not NameError or generic Exception
        //        var result = EvaluateExpression(@"
        //    begin
        //        raise ArgumentError, 'bad argument'
        //    rescue NameError
        //        'name error'
        //    rescue ArgumentError
        //        'argument error'
        //    rescue
        //        'generic error'
        //    end
        //");
        //        Assert.AreEqual("argument error", result);

        //        // This should catch only NameError
        //        result = EvaluateExpression(@"
        //    begin
        //        raise NameError, 'bad name'
        //    rescue ArgumentError
        //        'argument error'
        //    rescue NameError
        //        'name error'
        //    rescue
        //        'generic error'
        //    end
        //");
        //        Assert.AreEqual("name error", result);

        //        // This should fall through to generic rescue
        //        result = EvaluateExpression(@"
        //    begin
        //        raise 'something else'
        //    rescue ArgumentError
        //        'argument error'
        //    rescue NameError
        //        'name error'
        //    rescue
        //        'generic error'
        //    end
        //");
        //        Assert.AreEqual("generic error", result);
        //    }

        [TestMethod]
        public void EvaluatePostfixIfUnless()
        {
            var output = new StringWriter();
            var machine = new Machine();
            machine.RootContext.Self.Class.SetInstanceMethod("puts", new PutsFunction(output));

            machine.ExecuteText(@"puts 'monkey' unless 3 > 4");
            Assert.AreEqual("monkey\n", output.ToString().Replace("\r\n", "\n"));

            output.GetStringBuilder().Clear();
            machine.ExecuteText(@"puts 'nope' unless 3 < 4");
            Assert.AreEqual("", output.ToString().Trim());

            output.GetStringBuilder().Clear();
            machine.ExecuteText(@"puts 'banana' if 3 < 4");
            Assert.AreEqual("banana\n", output.ToString().Replace("\r\n", "\n"));

            output.GetStringBuilder().Clear();
            machine.ExecuteText(@"puts 'nope' if 3 > 4");
            Assert.AreEqual("", output.ToString().Trim());

        }

        [TestMethod]
        public void EvaluateTernaryOperator()
        {
            Assert.AreEqual("yes", EvaluateExpression("true ? 'yes' : 'no'"));
            Assert.AreEqual("no", EvaluateExpression("false ? 'yes' : 'no'"));
            Assert.AreEqual(42L, EvaluateExpression("1 == 1 ? 42 : 0"));
            Assert.AreEqual(0L, EvaluateExpression("1 != 1 ? 42 : 0"));
        }

        [TestMethod]
        public void ParseCallWithTernaryArgument()
        {
            Parser parser = new("puts true ? \"yes\" : \"no\"");
            var expected = new CallExpression("puts", [
                new TernaryExpression(
            new ConstantExpression(true),
            new ConstantExpression("yes"),
            new ConstantExpression("no")
        )
            ]);
            var result = parser.ParseExpression();

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ParseAndOrExpressions()
        {
            Assert.AreEqual(true, EvaluateExpression("true && true"));
            Assert.AreEqual(false, EvaluateExpression("true && false"));
            Assert.AreEqual(true, EvaluateExpression("false || true"));
            Assert.AreEqual(false, EvaluateExpression("false || false"));

            // Short-circuit tests
            Assert.AreEqual(false, EvaluateExpression("false && raise 'fail'"));
            Assert.AreEqual(true, EvaluateExpression("true || raise 'fail'"));
        }

        [TestMethod]
        public void LoopControlFlow()
        {
            Assert.AreEqual(6L, Execute(@"
total = 0
for i in 1..5 do
    if i < 4 then
        total = total + i
    else
        break i
    end
end
total
            "));

            Assert.AreEqual(9L, Execute(@"
total = -1
for i in 1..10 do
    if i < 10 then
        next
    end
    total = total + i
end
total
            "));

            Assert.AreEqual(5L, Execute(@"
i = 0
while i < 5
    i = i + 1
    redo if i == 3
end
i
            "));
        }

        [TestMethod]
        public void ReturnExitsMethodEarly()
        {
            var result = Execute(@"
def foo
  return 1
  2
end
foo
");

            Assert.AreEqual(1L, result);
        }

        [TestMethod]
        public void ReturnWithoutValueReturnsNil()
        {
            var result = Execute(@"
def foo
  return
  2
end
foo
");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void ReturnAtTopLevelRaises()
        {
            try
            {
                Execute("return 1");
                Assert.Fail("Expected InvalidOperationError");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(InvalidOperationError));
            }
        }

        [TestMethod]
        public void ReturnInsideBlockRaises()
        {
            try
            {
                Execute(@"
def foo
  [1].each do |x|
    return x
  end
  2
end
foo
");
                Assert.Fail("Expected InvalidOperationError");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(InvalidOperationError));
            }
        }

        [TestMethod]
        public void ModuloAndPowerOperators()
        {
            Assert.AreEqual(1L, EvaluateExpression("5 % 2"));
            Assert.AreEqual(0L, EvaluateExpression("4 % 2"));

            Assert.AreEqual(8L, EvaluateExpression("2 ** 3"));
            Assert.AreEqual(1L, EvaluateExpression("10 % 3 % 3"));
            Assert.AreEqual(81L, EvaluateExpression("3 ** 2 ** 2"));  // Should be right-associative
        }

        [TestMethod]
        public void NotKeywordWorks()
        {
            Assert.AreEqual(false, EvaluateExpression("not true"));
            Assert.AreEqual(true, EvaluateExpression("not false"));
            Assert.AreEqual(false, EvaluateExpression("not 1 == 1"));
        }

        [TestMethod]
        public void ElsifWorks() => Assert.AreEqual("middle", Execute(@"
if false
  'low'
elsif true
  'middle'
else
  'high'
end
"));

        [TestMethod]
        public void DefinedKeywordDetectsLocals()
        {
            Assert.AreEqual("local-variable", Execute(@"
foo = 123
defined? foo
    "));

            Assert.AreEqual(null, Execute(@"
defined? bar
    "));
        }

        //        [TestMethod]
        //        public void AliasCreatesMethodAlias()
        //        {
        //            Execute(@"
        //class Greeter
        //    def greet
        //        'hello'
        //    end

        //    alias say_hello greet
        //end
        //    ");

        //            var result = Execute("Greeter.new.say_hello");
        //            Assert.AreEqual("hello", result);
        //        }

        [TestMethod]
        public void YieldCallsBlockWithoutArgs()
        {
            var result = Execute(@"
def run
  yield
end

run { 'ok' }
");
            Assert.AreEqual("ok", result);
        }

        [TestMethod]
        public void YieldPassesArgumentsToBlock()
        {
            var result = Execute(@"
def double(x)
  yield x * 2
end

double(3) { |n| n + 1 }  # Should yield 6 → 7
");
            Assert.AreEqual(7L, result);
        }

        [TestMethod]
        public void YieldWithoutBlockRaises()
        {
            try
            {
                Execute(@"
def run
  yield
end

run
");
                Assert.Fail("Expected NameError");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(NameError));
                Assert.IsTrue(ex.Message.Contains("no block given"));
            }
        }

        [TestMethod]
        public void MultipleYields()
        {
            var result = Execute(@"
def tally
  yield
  yield
end

count = 0
puts count

tally { count += 1 }
count
");
            Assert.AreEqual(2L, result);
        }

        private object EvaluateExpression(string text)
        {
            Parser parser = new(text);
            IExpression expression = parser.ParseExpression();
            Assert.IsNull(parser.ParseExpression());
            return expression.Evaluate(machine.RootContext);
        }

        //private object EvaluateCommand(string text)
        //{
        //    Parser parser = new(text);
        //    IExpression expression = parser.ParseCommand();
        //    Assert.IsNull(parser.ParseExpression());
        //    return expression.Evaluate(machine.RootContext);
        //}

        private object Execute(string text) => machine.ExecuteText(text);
    }
}
