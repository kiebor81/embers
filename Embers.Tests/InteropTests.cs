using Embers.Language;
using Embers.Security;

namespace Embers.Tests
{
    /// <summary>
    /// Tests demonstrating C# interoperability in Embers.
    /// These tests show how the Embers interpreter can access .NET types
    /// and static members from the .NET framework.
    /// </summary>
    [TestClass]
    public class InteropTests
    {
        private Machine machine;

        [TestInitialize]
        public void Setup()
        {
            machine = new Machine();
            // Ensure unrestricted mode for these tests
            machine.SetTypeAccessPolicy([], SecurityMode.Unrestricted);
        }

        // ==================== Basic .NET Type Access ====================
        
        [TestMethod]
        public void AccessSystemDateTimeNow()
        {
            // Access a static property from a .NET type
            object result = machine.ExecuteText("System::DateTime.Now");
            
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(DateTime));
        }

        [TestMethod]
        public void CallSystemGuidNewGuid()
        {
            // Call a static method on a .NET type
            object result = machine.ExecuteText("System::Guid.NewGuid()");
            
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(Guid));
        }

        [TestMethod]
        public void CallSystemMathAbsolute()
        {
            // Call a static method on System::Math
            object result = machine.ExecuteText("System::Math.Abs(-42)");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(42L, result);
        }

        [TestMethod]
        public void AccessIntMaxValue()
        {
            // Access a static constant field from a .NET type
            object result = machine.ExecuteText("System::Int32.MaxValue");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(int.MaxValue, result);
        }

        // ==================== Chaining .NET Method Calls ====================

        [TestMethod]
        public void ChainSystemStringMethods()
        {
            // Chain methods from .NET types
            object result = machine.ExecuteText("System::String.Concat('Hello', ' ', 'World')");
            
            Assert.IsNotNull(result);
            Assert.AreEqual("Hello World", result);
        }

        [TestMethod]
        public void CallMethodOnDateTimeInstance()
        {
            // Get a .NET object and call instance methods on it
            object result = machine.ExecuteText("System::DateTime.Now.Year");
            
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(int));
        }

        // ==================== Multiple Type Access ====================

        [TestMethod]
        public void AccessMultipleDotNetTypes()
        {
            // Demonstrate accessing various .NET types in sequence
            object guid = machine.ExecuteText("System::Guid.NewGuid()");
            object now = machine.ExecuteText("System::DateTime.Now");
            object max = machine.ExecuteText("System::Int32.MaxValue");
            
            Assert.IsNotNull(guid);
            Assert.IsNotNull(now);
            Assert.IsNotNull(max);
        }

        // ==================== Mixed Syntax - Embers + C# Interop ====================

        [TestMethod]
        public void StoreNetObjectInEmbersVariable()
        {
            // Store a .NET object in an Embers variable
            machine.ExecuteText("guid = System::Guid.NewGuid()");
            
            object result = machine.RootContext.GetLocalValue("guid");
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(Guid));
        }

        [TestMethod]
        public void CallNetMethodOnEmbersVariable()
        {
            // Store .NET object and call methods on it
            machine.ExecuteText("dt = System::DateTime.Now");
            object year = machine.ExecuteText("dt.Year");
            
            Assert.IsNotNull(year);
            Assert.IsInstanceOfType(year, typeof(int));
        }

        [TestMethod]
        public void UseNetObjectInEmbersArray()
        {
            // Mix .NET objects with Embers arrays
            machine.ExecuteText("arr = [System::Guid.NewGuid(), System::DateTime.Now, 42]");
            
            object result = machine.RootContext.GetLocalValue("arr");
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void UseNetObjectInEmbersHash()
        {
            // Mix .NET objects in Embers hash
            machine.ExecuteText("data = { 'id' => System::Guid.NewGuid(), 'time' => System::DateTime.Now }");
            
            object result = machine.RootContext.GetLocalValue("data");
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void PassNetObjectToEmbersFunction()
        {
            // Define an Embers function that works with .NET objects
            machine.ExecuteText(@"
                def get_year(dt)
                    dt.Year
                end
            ");
            
            object result = machine.ExecuteText("get_year(System::DateTime.Now)");
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(int));
        }

        [TestMethod]
        public void UseNetObjectInEmbersConditional()
        {
            // Use .NET object in Embers if statement
            object result = machine.ExecuteText(@"
                num = System::Math.Abs(-10)
                if num > 5
                    'big'
                else
                    'small'
                end
            ");
            
            Assert.AreEqual("big", result);
        }

        [TestMethod]
        public void UseNetObjectInEmbersLoop()
        {
            // Use .NET object in Embers loop
            machine.ExecuteText(@"
                max = System::Int32.MaxValue
                count = 0
                for i in 1..3
                    if i < max
                        count = count + 1
                    end
                end
            ");
            
            object result = machine.RootContext.GetLocalValue("count");
            Assert.AreEqual(3L, result);
        }

        [TestMethod]
        public void ChainEmbersMethodsWithNetObjects()
        {
            // Chain Embers and .NET methods together
            machine.ExecuteText("values = [1, -2, 3, -4, 5]");
            object result = machine.ExecuteText(@"
                values.map do |x|
                    System::Math.Abs(x)
                end
            ");
            
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void UseNetMathInEmbersCalculation()
        {
            // Mix .NET Math methods with Embers arithmetic
            object result = machine.ExecuteText(@"
                a = System::Math.Abs(-15)
                b = System::Math.Sqrt(16)
                a + b
            ");
            
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void FormatNetDateTimeWithEmbersString()
        {
            // Get .NET DateTime and format with Embers string operations
            machine.ExecuteText("dt = System::DateTime.Now");
            object year = machine.ExecuteText("dt.Year.ToString()");
            
            Assert.IsNotNull(year);
            Assert.IsInstanceOfType(year, typeof(string));
        }

        // ==================== Enum Access ====================

        [TestMethod]
        public void AccessEnumValue()
        {
            // Access .NET enum values using :: operator
            object result = machine.ExecuteText("System::DayOfWeek::Monday");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(DayOfWeek.Monday, result);
        }

        [TestMethod]
        public void UseEnumInComparison()
        {
            // Use enum in Embers comparison
            object result = machine.ExecuteText(@"
                today = System::DateTime.Now.DayOfWeek
                if today == System::DayOfWeek::Saturday || today == System::DayOfWeek::Sunday
                    'weekend'
                else
                    'weekday'
                end
            ");
            
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Equals("weekend") || result.Equals("weekday"));
        }

        // ==================== Namespace Separator Syntax ====================

        [TestMethod]
        public void AccessTypeWithDoubleColon()
        {
            // Namespace separation with :: and member access with .
            object result = machine.ExecuteText("System::DateTime.Now");
            
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(DateTime));
        }

        [TestMethod]
        public void CallStaticMethodWithDoubleColon()
        {
            // Namespace :: and static field access .
            object result = machine.ExecuteText("System::String.Empty");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void AccessStaticFieldWithDoubleColon()
        {
            // Namespace :: and static field access .
            object result = machine.ExecuteText("System::Int32.MaxValue");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(int.MaxValue, result);
        }

        [TestMethod]
        public void CallMathMethodWithDoubleColon()
        {
            // Namespace :: and constant access
            object result = machine.ExecuteText("System::Math.PI");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(Math.PI, result);
        }

        [TestMethod]
        public void StoreDoubleColonTypeInVariable()
        {
            // Store type accessed with :: namespace separator
            machine.ExecuteText("dt = System::DateTime.Now");
            
            object result = machine.RootContext.GetLocalValue("dt");
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(DateTime));
        }

        [TestMethod]
        public void UseDoubleColonTypeInArray()
        {
            // Use :: namespace syntax in array
            machine.ExecuteText("arr = [System::DateTime.Now, System::Int32.MaxValue]");
            
            object result = machine.RootContext.GetLocalValue("arr");
            Assert.IsNotNull(result);
        }

        // ==================== Type Access Policy Configuration ====================

        [TestMethod]
        public void DemonstrateUnrestrictedMode()
        {
            // In unrestricted mode, all .NET types are accessible
            machine.SetTypeAccessPolicy([], SecurityMode.Unrestricted);
            
            object result = machine.ExecuteText("System::DateTime.Now");
            Assert.IsNotNull(result);
        }
    }
}
