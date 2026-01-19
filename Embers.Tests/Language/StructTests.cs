using Embers.Exceptions;

namespace Embers.Tests.Language
{
    [TestClass]
    public class StructTests
    {
        [TestMethod]
        public void Struct_New_CreatesAccessors()
        {
            Machine machine = new();
            machine.ExecuteText("Point = Struct.new(:x, :y)");
            var result = machine.ExecuteText("p = Point.new(1, 2); p.x");

            Assert.IsNotNull(result);
            Assert.AreEqual(1L, result);
        }

        [TestMethod]
        public void Struct_Setter_UpdatesValue()
        {
            Machine machine = new();
            machine.ExecuteText("Point = Struct.new(:x, :y)");
            var result = machine.ExecuteText("p = Point.new(1, 2); p.x = 10; p.x");

            Assert.IsNotNull(result);
            Assert.AreEqual(10L, result);
        }

        [TestMethod]
        public void Struct_Named_RegistersConstant()
        {
            Machine machine = new();
            machine.ExecuteText("Struct.new('Point', :x, :y)");
            var result = machine.ExecuteText("Point.new(3, 4).y");

            Assert.IsNotNull(result);
            Assert.AreEqual(4L, result);
        }

        [TestMethod]
        public void Struct_TooManyArguments_Raises()
        {
            Machine machine = new();
            machine.ExecuteText("Point = Struct.new(:x)");

            var ex = Assert.ThrowsException<ArgumentError>(() => machine.ExecuteText("Point.new(1, 2)"));
            Assert.AreEqual("wrong number of arguments (given 2, expected 1)", ex.Message);
        }

        [TestMethod]
        public void Struct_Members_ReturnsSymbols()
        {
            Machine machine = new();
            machine.ExecuteText("Point = Struct.new(:x, :y)");
            var result = machine.ExecuteText("Point.members");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(DynamicArray));
            var list = (DynamicArray)result;

            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(new Symbol("x"), list[0]);
            Assert.AreEqual(new Symbol("y"), list[1]);
        }
    }
}
