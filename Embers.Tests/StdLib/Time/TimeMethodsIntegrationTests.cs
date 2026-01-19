namespace Embers.Tests.StdLib.Time
{
    [TestClass]
    public class TimeMethodsIntegrationTests
    {
        private Machine machine;

        [TestInitialize]
        public void Setup() => machine = new Machine();

        [TestMethod]
        public void Now_GlobalFunction_ReturnsDateTime()
        {
            var result = machine.ExecuteText("now");
            Assert.IsInstanceOfType(result, typeof(DateTime));
        }

        [TestMethod]
        public void DateAndTimeConstants_AliasDateTime()
        {
            var dateTimeClass = machine.RootContext.GetValue("DateTime");
            var timeClass = machine.RootContext.GetValue("Time");
            var dateClass = machine.RootContext.GetValue("Date");

            Assert.AreSame(dateTimeClass, timeClass);
            Assert.AreSame(dateTimeClass, dateClass);
        }

        [TestMethod]
        public void Today_GlobalFunction_ReturnsDateTime()
        {
            var result = machine.ExecuteText("today");
            Assert.IsInstanceOfType(result, typeof(DateTime));
            var dt = (DateTime)result;
            Assert.AreEqual(0, dt.Hour);
            Assert.AreEqual(0, dt.Minute);
            Assert.AreEqual(0, dt.Second);
        }

        [TestMethod]
        public void Epoch_GlobalFunction_ReturnsNumber()
        {
            var result = machine.ExecuteText("epoch");
            Assert.IsInstanceOfType(result, typeof(long));
        }

        [TestMethod]
        public void FromEpoch_GlobalFunction_ReturnsDateTime()
        {
            var result = machine.ExecuteText("from_epoch(0)");
            Assert.IsInstanceOfType(result, typeof(DateTime));
            var dt = (DateTime)result;
            Assert.AreEqual(1970, dt.Year);
            Assert.AreEqual(1, dt.Month);
            Assert.AreEqual(1, dt.Day);
        }

        [TestMethod]
        public void ParseDate_GlobalFunction_ParsesString()
        {
            var result = machine.ExecuteText("parse_date('2026-01-10')");
            Assert.IsInstanceOfType(result, typeof(DateTime));
            var dt = (DateTime)result;
            Assert.AreEqual(2026, dt.Year);
            Assert.AreEqual(1, dt.Month);
            Assert.AreEqual(10, dt.Day);
        }

        [TestMethod]
        public void DateTime_YearMethod_ReturnsYear()
        {
            var result = machine.ExecuteText("dt = parse_date('2026-01-10'); dt.year");
            Assert.AreEqual(2026, result);
        }

        [TestMethod]
        public void DateTime_MonthMethod_ReturnsMonth()
        {
            var result = machine.ExecuteText("dt = parse_date('2026-01-10'); dt.month");
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void DateTime_DayMethod_ReturnsDay()
        {
            var result = machine.ExecuteText("dt = parse_date('2026-01-10'); dt.day");
            Assert.AreEqual(10, result);
        }

        [TestMethod]
        public void DateTime_HourMethod_ReturnsHour()
        {
            var result = machine.ExecuteText("dt = parse_date('2026-01-10 14:30:00'); dt.hour");
            Assert.AreEqual(14, result);
        }

        [TestMethod]
        public void DateTime_MinMethod_ReturnsMinute()
        {
            var result = machine.ExecuteText("dt = parse_date('2026-01-10 14:30:45'); dt.min");
            Assert.AreEqual(30, result);
        }

        [TestMethod]
        public void DateTime_SecMethod_ReturnsSecond()
        {
            var result = machine.ExecuteText("dt = parse_date('2026-01-10 14:30:45'); dt.sec");
            Assert.AreEqual(45, result);
        }

        [TestMethod]
        public void DateTime_UtcMethod_ReturnsDateTime()
        {
            var result = machine.ExecuteText("dt = parse_date('2026-01-10 14:30:45'); dt.utc");
            Assert.IsInstanceOfType(result, typeof(DateTime));
            var dt = (DateTime)result;
            Assert.AreEqual(2026, dt.Year);
        }

        [TestMethod]
        public void DateTime_StrftimeMethod_FormatsDate()
        {
            var result = machine.ExecuteText("dt = parse_date('2026-01-10'); dt.strftime('yyyy-MM-dd')");
            Assert.AreEqual("2026-01-10", result);
        }

        [TestMethod]
        public void DateTime_ToEpochMethod_ReturnsSeconds()
        {
            var result = machine.ExecuteText("dt = parse_date('1970-01-01 00:00:00'); dt.utc.to_epoch");
            Assert.AreEqual(0L, result);
        }

        [TestMethod]
        public void DateTime_FromEpochToEpoch_RoundTrip()
        {
            var result = machine.ExecuteText("from_epoch(10).to_epoch");
            Assert.AreEqual(10L, result);
        }

        [TestMethod]
        public void DateTime_StrftimeMethod_FormatsDateTime()
        {
            var result = machine.ExecuteText("dt = parse_date('2026-01-10 14:30:45'); dt.strftime('yyyy-MM-dd HH:mm:ss')");
            Assert.AreEqual("2026-01-10 14:30:45", result);
        }

        [TestMethod]
        public void DateTime_MethodChaining_WorksCorrectly()
        {
            var result = machine.ExecuteText("(parse_date('2026-01-10')).year");
            Assert.AreEqual(2026, result);
        }

        [TestMethod]
        public void DateTime_MultipleProperties_WorkInExpression()
        {
            var result = machine.ExecuteText(@"
                dt = parse_date('2026-01-10')
                year = dt.year
                month = dt.month
                day = dt.day
                [year, month, day]
            ");
            var list = (DynamicArray)result;
            Assert.AreEqual(2026, list[0]);
            Assert.AreEqual(1, list[1]);
            Assert.AreEqual(10, list[2]);
        }
    }
}
