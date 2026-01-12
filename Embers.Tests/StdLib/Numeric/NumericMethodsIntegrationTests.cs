namespace Embers.Tests.StdLib.Numeric
{
    /// <summary>
    /// Integration tests for Numeric methods showing both Fixnum and Float usage patterns.
    /// These demonstrate that numeric methods work as instance methods on both types.
    /// </summary>
    [TestClass]
    public class NumericMethodsIntegrationTests
    {
        // ==================== ABS METHOD TESTS ====================

        [TestMethod]
        public void Fixnum_AbsMethod_PositiveNumber_ReturnsSame()
        {
            Machine machine = new();
            var result = machine.ExecuteText("5.abs");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(5L, result);
        }

        [TestMethod]
        public void Fixnum_AbsMethod_NegativeNumber_ReturnsPositive()
        {
            Machine machine = new();
            // Using indirect negative to avoid parsing issues
            var result = machine.ExecuteText("x = -5\nx.abs");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(5L, result);
        }

        [TestMethod]
        public void Float_AbsMethod_PositiveNumber_ReturnsSame()
        {
            Machine machine = new();
            var result = machine.ExecuteText("3.14.abs");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(3.14, result);
        }

        // ==================== CEIL METHOD TESTS ====================

        [TestMethod]
        public void Fixnum_CeilMethod_ReturnsInteger()
        {
            Machine machine = new();
            var result = machine.ExecuteText("5.ceil");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(5L, result);
        }

        [TestMethod]
        public void Float_CeilMethod_RoundsUp()
        {
            Machine machine = new();
            var result = machine.ExecuteText("3.14.ceil");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(4L, result);
        }

        // ==================== FLOOR METHOD TESTS ====================

        [TestMethod]
        public void Fixnum_FloorMethod_ReturnsInteger()
        {
            Machine machine = new();
            var result = machine.ExecuteText("5.floor");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(5L, result);
        }

        [TestMethod]
        public void Float_FloorMethod_RoundsDown()
        {
            Machine machine = new();
            var result = machine.ExecuteText("3.14.floor");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(3L, result);
        }

        // ==================== ROUND METHOD TESTS ====================

        [TestMethod]
        public void Fixnum_RoundMethod_ReturnsInteger()
        {
            Machine machine = new();
            var result = machine.ExecuteText("5.round");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(5L, result);
        }

        [TestMethod]
        public void Float_RoundMethod_RoundsDown()
        {
            Machine machine = new();
            var result = machine.ExecuteText("3.14.round");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(3L, result);
        }

        [TestMethod]
        public void Float_RoundMethod_RoundsUp()
        {
            Machine machine = new();
            var result = machine.ExecuteText("3.78.round");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(4L, result);
        }

        // ==================== SQRT METHOD TESTS ====================

        [TestMethod]
        public void Fixnum_SqrtMethod_PerfectSquare()
        {
            Machine machine = new();
            var result = machine.ExecuteText("9.sqrt");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(3.0, result);
        }

        [TestMethod]
        public void Fixnum_SqrtMethod_NonPerfectSquare()
        {
            Machine machine = new();
            var result = machine.ExecuteText("2.sqrt");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(Math.Sqrt(2), result);
        }

        [TestMethod]
        public void Float_SqrtMethod_ReturnsSquareRoot()
        {
            Machine machine = new();
            var result = machine.ExecuteText("2.25.sqrt");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(1.5, result);
        }

        // ==================== SIN METHOD TESTS ====================

        [TestMethod]
        public void Fixnum_SinMethod_Zero()
        {
            Machine machine = new();
            var result = machine.ExecuteText("0.sin");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(0.0, result);
        }

        [TestMethod]
        public void Float_SinMethod_PiOver2()
        {
            Machine machine = new();
            var result = machine.ExecuteText("1.5707963267948966.sin");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(1.0, (double)result, 0.0000001);
        }

        // ==================== COS METHOD TESTS ====================

        [TestMethod]
        public void Fixnum_CosMethod_Zero()
        {
            Machine machine = new();
            var result = machine.ExecuteText("0.cos");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(1.0, result);
        }

        // ==================== POW METHOD TESTS ====================

        [TestMethod]
        public void Fixnum_PowMethod_SquaresNumber()
        {
            Machine machine = new();
            var result = machine.ExecuteText("5.pow(2)");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(25.0, result);
        }

        [TestMethod]
        public void Float_PowMethod_CubesNumber()
        {
            Machine machine = new();
            var result = machine.ExecuteText("2.5.pow(3)");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(15.625, result);
        }

        // ==================== MIN/MAX METHOD TESTS ====================

        [TestMethod]
        public void Fixnum_MinMethod_ReturnsSmallerValue()
        {
            Machine machine = new();
            var result = machine.ExecuteText("5.min(3)");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(3.0, result);
        }

        [TestMethod]
        public void Float_MaxMethod_ReturnsLargerValue()
        {
            Machine machine = new();
            var result = machine.ExecuteText("3.14.max(2.71)");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(3.14, result);
        }

        // ==================== SIGN METHOD TESTS ====================

        [TestMethod]
        public void Fixnum_SignMethod_PositiveNumber()
        {
            Machine machine = new();
            var result = machine.ExecuteText("5.sign");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void Fixnum_SignMethod_NegativeNumber()
        {
            Machine machine = new();
            var result = machine.ExecuteText("(-5).sign");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(-1, result);
        }

        [TestMethod]
        public void Float_SignMethod_Zero()
        {
            Machine machine = new();
            var result = machine.ExecuteText("0.0.sign");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result);
        }

        // ==================== DEGREE/RADIAN METHOD TESTS ====================

        [TestMethod]
        public void Float_DegreeMethod_ConvertsRadiansToDegrees()
        {
            Machine machine = new();
            var result = machine.ExecuteText("3.14159265358979.degree");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(180.0, (double)result, 0.0001);
        }

        [TestMethod]
        public void Fixnum_RadianMethod_ConvertsDegToRadians()
        {
            Machine machine = new();
            var result = machine.ExecuteText("180.radian");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(Math.PI, (double)result, 0.0001);
        }
    }
}
