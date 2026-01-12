namespace Embers.Tests.StdLib.Conversion
{
    /// <summary>
    /// Integration tests for Conversion methods showing usage patterns across different types.
    /// </summary>
    [TestClass]
    public class ConversionMethodsIntegrationTests
    {
        private Machine machine;

        [TestInitialize]
        public void Setup()
        {
            machine = new Machine();
        }

        // to_i tests
        [TestMethod]
        public void Fixnum_ToIMethod_ReturnsSelf()
        {
            var result = machine.ExecuteText("42.to_i");
            Assert.AreEqual(42L, result);
        }

        [TestMethod]
        public void Float_ToIMethod_TruncatesToInteger()
        {
            var result = machine.ExecuteText("3.14.to_i");
            Assert.AreEqual(3L, result);
        }

        [TestMethod]
        public void Float_ToIMethod_NegativeNumber()
        {
            var result = machine.ExecuteText("(-3.99).to_i");
            Assert.AreEqual(-3L, result);
        }

        [TestMethod]
        public void String_ToIMethod_ParsesInteger()
        {
            var result = machine.ExecuteText("'42'.to_i");
            Assert.AreEqual(42L, result);
        }

        [TestMethod]
        public void String_ToIMethod_ParsesNegative()
        {
            var result = machine.ExecuteText("'-123'.to_i");
            Assert.AreEqual(-123L, result);
        }

        [TestMethod]
        public void True_ToIMethod_ReturnsOne()
        {
            var result = machine.ExecuteText("true.to_i");
            Assert.AreEqual(1L, result);
        }

        [TestMethod]
        public void False_ToIMethod_ReturnsZero()
        {
            var result = machine.ExecuteText("false.to_i");
            Assert.AreEqual(0L, result);
        }

        // to_f tests
        [TestMethod]
        public void Fixnum_ToFMethod_ConvertsToFloat()
        {
            var result = machine.ExecuteText("42.to_f");
            Assert.AreEqual(42.0, result);
        }

        [TestMethod]
        public void Float_ToFMethod_ReturnsSelf()
        {
            var result = machine.ExecuteText("3.14.to_f");
            Assert.AreEqual(3.14, result);
        }

        [TestMethod]
        public void String_ToFMethod_ParsesFloat()
        {
            var result = machine.ExecuteText("'3.14'.to_f");
            Assert.AreEqual(3.14, result);
        }

        [TestMethod]
        public void String_ToFMethod_ParsesInteger()
        {
            var result = machine.ExecuteText("'42'.to_f");
            Assert.AreEqual(42.0, result);
        }

        [TestMethod]
        public void True_ToFMethod_ReturnsOne()
        {
            var result = machine.ExecuteText("true.to_f");
            Assert.AreEqual(1.0, result);
        }

        [TestMethod]
        public void False_ToFMethod_ReturnsZero()
        {
            var result = machine.ExecuteText("false.to_f");
            Assert.AreEqual(0.0, result);
        }

        // to_s tests
        [TestMethod]
        public void Fixnum_ToSMethod_ConvertsToString()
        {
            var result = machine.ExecuteText("42.to_s");
            Assert.AreEqual("42", result);
        }

        [TestMethod]
        public void Float_ToSMethod_ConvertsToString()
        {
            var result = machine.ExecuteText("3.14.to_s");
            Assert.AreEqual("3.14", result);
        }

        [TestMethod]
        public void String_ToSMethod_ReturnsSelf()
        {
            var result = machine.ExecuteText("'hello'.to_s");
            Assert.AreEqual("hello", result);
        }

        [TestMethod]
        public void True_ToSMethod_ReturnsTrue()
        {
            var result = machine.ExecuteText("true.to_s");
            Assert.AreEqual("True", result);
        }

        [TestMethod]
        public void False_ToSMethod_ReturnsFalse()
        {
            var result = machine.ExecuteText("false.to_s");
            Assert.AreEqual("False", result);
        }

        [TestMethod]
        public void Array_ToSMethod_ReturnsRepresentation()
        {
            var result = machine.ExecuteText("[1, 2, 3].to_s");
            Assert.IsInstanceOfType(result, typeof(string));
            Assert.IsTrue(((string)result).Contains("1"));
        }

        [TestMethod]
        public void DateTime_ToSMethod_ReturnsDateTimeString()
        {
            var result = machine.ExecuteText("(parse_date('2026-01-10')).to_s");
            Assert.IsInstanceOfType(result, typeof(string));
            Assert.IsTrue(((string)result).Contains("2026"));
        }

        // Chaining tests
        [TestMethod]
        public void ConversionChaining_StringToIntToFloat()
        {
            var result = machine.ExecuteText("'42'.to_i.to_f");
            Assert.AreEqual(42.0, result);
        }

        [TestMethod]
        public void ConversionChaining_FloatToIntToString()
        {
            var result = machine.ExecuteText("3.99.to_i.to_s");
            Assert.AreEqual("3", result);
        }

        [TestMethod]
        public void ConversionInExpression_UsedInCalculation()
        {
            var result = machine.ExecuteText("'10'.to_i + '20'.to_i");
            Assert.AreEqual(30L, result);
        }

        [TestMethod]
        public void ConversionInExpression_MixedTypes()
        {
            var result = machine.ExecuteText("5.to_f + '2.5'.to_f");
            Assert.AreEqual(7.5, result);
        }

        // Alias tests
        [TestMethod]
        public void ToIntAlias_WorksCorrectly()
        {
            var result = machine.ExecuteText("'42'.to_int");
            Assert.AreEqual(42L, result);
        }

        [TestMethod]
        public void ToFloatAlias_WorksCorrectly()
        {
            var result = machine.ExecuteText("42.to_float");
            Assert.AreEqual(42.0, result);
        }

        [TestMethod]
        public void ToStringAlias_WorksCorrectly()
        {
            var result = machine.ExecuteText("42.to_string");
            Assert.AreEqual("42", result);
        }
    }
}
