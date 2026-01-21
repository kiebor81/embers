using Embers.Exceptions;

namespace Embers.Tests.Runtime.Language
{
    [TestClass]
    public class MetaMethodsTests
    {
        private Machine machine;

        [TestInitialize]
        public void Setup() => machine = new Machine();

        [TestMethod]
        public void InstanceVariableGet_ReturnsValue()
        {
            var result = machine.ExecuteText("class MyClass; def initialize; @foo = 1; end; end; obj = MyClass.new; obj.instance_variable_get(:@foo)");
            Assert.AreEqual(1L, result);
        }

        [TestMethod]
        public void InstanceVariableGet_MissingReturnsNil()
        {
            var result = machine.ExecuteText("Object.new.instance_variable_get(:@missing)");
            Assert.IsNull(result);
        }

        [TestMethod]
        public void InstanceVariableSet_RoundTrip()
        {
            var result = machine.ExecuteText("obj = Object.new; obj.instance_variable_set('@bar', 2); obj.instance_variable_get(:@bar)");
            Assert.AreEqual(2L, result);
        }

        [TestMethod]
        public void InstanceVariables_ListsNames()
        {
            var result = machine.ExecuteText("obj = Object.new; obj.instance_variable_set(:@foo, 1); obj.instance_variables");
            Assert.IsInstanceOfType(result, typeof(DynamicArray));
            Assert.IsTrue(((DynamicArray)result).Contains(new Symbol("@foo")));
        }

        [TestMethod]
        public void ConstGetSet_RoundTrip()
        {
            var result = machine.ExecuteText("class MyClass; end; MyClass.const_set(:FOO, 3); MyClass.const_get('FOO')");
            Assert.AreEqual(3L, result);
        }

        [TestMethod]
        public void ConstGet_Missing_ThrowsNameError()
        {
            Assert.ThrowsException<NameError>(() =>
                machine.ExecuteText("class MyClass; end; MyClass.const_get(:MISSING)"));
        }

        [TestMethod]
        public void ConstSet_InvalidName_ThrowsNameError()
        {
            Assert.ThrowsException<NameError>(() =>
                machine.ExecuteText("class MyClass; end; MyClass.const_set(:foo, 1)"));
        }

        [TestMethod]
        public void AliasMethod_WiresAlias()
        {
            var result = machine.ExecuteText("class MyClass; def foo; 5; end; alias_method :bar, :foo; end; MyClass.new.bar");
            Assert.AreEqual(5L, result);
        }

        [TestMethod]
        public void AliasMethod_MissingMethod_ThrowsNameError()
        {
            Assert.ThrowsException<NameError>(() =>
                machine.ExecuteText("class MyClass; alias_method :bar, :nope; end"));
        }

        [TestMethod]
        public void Send_DispatchesBySymbol()
        {
            var result = machine.ExecuteText("class MyClass; def add(a, b); a + b; end; end; MyClass.new.send(:add, 1, 2)");
            Assert.AreEqual(3L, result);
        }

        [TestMethod]
        public void Send_DispatchesByString()
        {
            var result = machine.ExecuteText("class MyClass; def add(a, b); a + b; end; end; MyClass.new.send('add', 1, 2)");
            Assert.AreEqual(3L, result);
        }

        [TestMethod]
        public void Send_MissingMethod_ThrowsNoMethodError()
        {
            Assert.ThrowsException<NoMethodError>(() =>
                machine.ExecuteText("Object.new.send(:missing)"));
        }

        [TestMethod]
        public void ClassVariables_ListsNames()
        {
            var result = machine.ExecuteText("class MyClass; @@foo = 1; end; MyClass.class_variables");
            Assert.IsInstanceOfType(result, typeof(DynamicArray));
            Assert.IsTrue(((DynamicArray)result).Contains(new Symbol("@@foo")));
        }

        [TestMethod]
        public void MethodMissing_Dispatches()
        {
            var result = machine.ExecuteText("class MyClass; def method_missing(name, a, b); name.to_s + ':' + (a + b).to_s; end; end; MyClass.new.foo(1, 2)");
            Assert.AreEqual("foo:3", result);
        }

        [TestMethod]
        public void MethodMissing_HandlesSend()
        {
            var result = machine.ExecuteText("class MyClass; def method_missing(name); 'ok'; end; end; MyClass.new.send(:missing)");
            Assert.AreEqual("ok", result);
        }

        [TestMethod]
        public void MethodMissing_RecursionGuard_ThrowsNoMethodError()
        {
            Assert.ThrowsException<NoMethodError>(() =>
                machine.ExecuteText("class MyClass; def method_missing(name); send(name); end; end; MyClass.new.foo"));
        }

        [TestMethod]
        public void DefineMethod_WithBlock_BindsSelf()
        {
            var result = machine.ExecuteText("Object.define_method(:set_x) { |v| @x = v }; Object.define_method(:get_x) { @x }; set_x(7); get_x");
            Assert.AreEqual(7L, result);
        }

        [TestMethod]
        public void DefineMethod_WithBlock_CapturesClosure()
        {
            var result = machine.ExecuteText("x = 4; Object.define_method(:add_x) { |y| x + y }; add_x(1)");
            Assert.AreEqual(5L, result);
        }

        [TestMethod]
        public void DefineMethod_WithProc_CapturesClosure()
        {
            var result = machine.ExecuteText("x = 2; f = lambda { |y| x + y }; Object.define_method(:add_x, f); add_x(3)");
            Assert.AreEqual(5L, result);
        }

        [TestMethod]
        public void DefineMethod_RequiresProcOrBlock()
        {
            Assert.ThrowsException<ArgumentError>(() => machine.ExecuteText("Object.define_method(:nope)"));
        }
    }
}
