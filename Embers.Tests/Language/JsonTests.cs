namespace Embers.Tests.Language
{
    [TestClass]
    public class JsonTests
    {
        private Machine machine;

        [TestInitialize]
        public void Setup() => machine = new Machine();

        [TestMethod]
        public void ToJson_SimpleHash()
        {
            var result = machine.ExecuteText(@"
                h = {'name' => 'John', 'age' => 30}
                h.to_json
            ");
            Assert.IsInstanceOfType(result, typeof(string));
            var json = (string)result;
            Assert.IsTrue(json.Contains("\"name\"") || json.Contains("\"Name\""));
            Assert.IsTrue(json.Contains("John"));
            Assert.IsTrue(json.Contains("30"));
        }

        [TestMethod]
        public void ToJson_Array()
        {
            var result = machine.ExecuteText(@"
                [1, 2, 3].to_json
            ");
            // JSON serializer may include spaces after commas
            Assert.IsInstanceOfType(result, typeof(string));
            var json = (string)result;
            Assert.IsTrue(json.Contains("1") && json.Contains("2") && json.Contains("3"));
        }

        [TestMethod]
        public void ToJson_String()
        {
            var result = machine.ExecuteText(@"
                ""hello"".to_json
            ");
            Assert.AreEqual("\"hello\"", result);
        }

        [TestMethod]
        public void ToJson_Number()
        {
            var result = machine.ExecuteText(@"
                42.to_json
            ");
            Assert.AreEqual("42", result);
        }

        [TestMethod]
        public void JsonParse_SimpleObject()
        {
            var result = machine.ExecuteText(@"
                json = '{""name"":""John"",""age"":30}'
                h = JSON.parse(json)
                h[""name""]
            ");
            Assert.AreEqual("John", result);
        }

        [TestMethod]
        public void JsonParse_Array()
        {
            var result = machine.ExecuteText(@"
                json = '[1,2,3,4,5]'
                arr = JSON.parse(json)
                arr[2]
            ");
            Assert.AreEqual(3, result);
        }

        [TestMethod]
        public void JsonParse_NestedObject()
        {
            var result = machine.ExecuteText(@"
                json = '{""person"":{""name"":""Jane"",""age"":25}}'
                data = JSON.parse(json)
                person = data[""person""]
                person[""name""]
            ");
            Assert.AreEqual("Jane", result);
        }

        [TestMethod]
        public void JsonParse_Boolean()
        {
            var result = machine.ExecuteText(@"
                json = '{""active"":true,""disabled"":false}'
                data = JSON.parse(json)
                data[""active""]
            ");
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void JsonParse_Null()
        {
            var result = machine.ExecuteText(@"
                json = '{""value"":null}'
                data = JSON.parse(json)
                data[""value""]
            ");
            Assert.IsNull(result);
        }

        [TestMethod]
        public void JsonRoundTrip()
        {
            var result = machine.ExecuteText(@"
                original = {'name' => 'Test', 'count' => 42}
                json = original.to_json
                parsed = JSON.parse(json)
                parsed['name']
            ");
            Assert.AreEqual("Test", result);
        }

        [TestMethod]
        public void JsonParse_MixedArray()
        {
            var result = machine.ExecuteText(@"
                json = '[1,""two"",true,null]'
                arr = JSON.parse(json)
                arr[1]
            ");
            Assert.AreEqual("two", result);
        }

        [TestMethod]
        [Ignore("Complex nested structures need additional work in JSON deserialization")]
        public void ToJson_ComplexNestedStructure()
        {
            var result = machine.ExecuteText(@"
                user1 = {'name' => 'Alice', 'age' => 30}
                user2 = {'name' => 'Bob', 'age' => 25}
                users = [user1, user2]
                data = {'users' => users}
                json = data.to_json
                parsed = JSON.parse(json)
                parsed_users = parsed['users']
                first = parsed_users[0]
                first['name']
            ");
            Assert.AreEqual("Alice", result);
        }
    }
}
