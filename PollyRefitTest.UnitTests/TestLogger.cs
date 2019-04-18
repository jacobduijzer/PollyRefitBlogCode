using System;
using PollyRefitTest.Logging;
using Xunit.Abstractions;

namespace PollyRefitTest.UnitTests
{
    public class TestLogger : ILogger
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public TestLogger(ITestOutputHelper testOutputHelper) =>
            _testOutputHelper = testOutputHelper;

        public void Debug(string message) =>
            _testOutputHelper.WriteLine(message);

        public void Debug(Exception exception, string message)
        {
            Debug(message);
            _testOutputHelper.WriteLine(exception.Message);
        }
    }
}
