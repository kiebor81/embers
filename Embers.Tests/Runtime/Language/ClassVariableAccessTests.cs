using Embers.Exceptions;

namespace Embers.Tests
{
    [TestClass]
    public class ClassVariableAccessTests
    {
        private Machine machine;

        [TestInitialize]
        public void Setup() => machine = new Machine();

        [TestMethod]
        public void ClassVariableGet_MissingPrefix_ThrowsNameError()
        {
            Assert.ThrowsException<NameError>(() =>
                machine.ExecuteText("class MyClass; @@value_0 = 1; end; MyClass.class_variable_get('value_0')"));
        }

        [TestMethod]
        public void ClassVariableSet_MissingPrefix_ThrowsNameError()
        {
            Assert.ThrowsException<NameError>(() =>
                machine.ExecuteText("class MyClass; end; MyClass.class_variable_set('value_0', 1)"));
        }

        [TestMethod]
        public void ClassVariableGet_WrongArity_ThrowsArgumentError()
        {
            Assert.ThrowsException<ArgumentError>(() =>
                machine.ExecuteText("class MyClass; end; MyClass.class_variable_get()"));
        }

        [TestMethod]
        public void ClassVariableSet_WrongArity_ThrowsArgumentError()
        {
            Assert.ThrowsException<ArgumentError>(() =>
                machine.ExecuteText("class MyClass; end; MyClass.class_variable_set('@@value_0')"));
        }

        [TestMethod]
        public void ClassVariableGet_NonStringOrSymbol_ThrowsTypeError()
        {
            Assert.ThrowsException<TypeError>(() =>
                machine.ExecuteText("class MyClass; end; MyClass.class_variable_get(123)"));
        }

        [TestMethod]
        public void ClassVariableSet_NonStringOrSymbol_ThrowsTypeError()
        {
            Assert.ThrowsException<TypeError>(() =>
                machine.ExecuteText("class MyClass; end; MyClass.class_variable_set(123, 1)"));
        }
    }
}
