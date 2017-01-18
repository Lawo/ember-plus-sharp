////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.Diagnostics.Tracing
{
    using System.Collections.ObjectModel;
    using System.Diagnostics.Tracing;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Reflection;
    using Threading;
    using Threading.Tasks;
    using UnitTesting;

    /// <summary>Tests the <see cref="Log"/> class.</summary>
    [TestClass]
    public sealed class LogTest : TestBase
    {
        /// <summary>Tests the content of a log event.</summary>
        [TestMethod]
        public void LogEventTest()
        {
            AsyncPump.Run(
                async () =>
                {
                    TestLogListener listener = new TestLogListener(EventLevel.Verbose);

                    string logMessage = "Test the debug log";
                    string moduleName = "LawoTest";

                    Log.Debug(logMessage, moduleName);

                    var logEvent = await WaitForLogEventAsync(listener.LogEvents, logMessage);

                    Assert.AreEqual(EventLevel.Verbose, logEvent.EventLevel);
                    Assert.AreEqual(1, logEvent.EventId);

                    Assert.AreEqual(logMessage, logEvent.LogMessage);
                    Assert.AreEqual(NativeMethods.GetCurrentThreadId(), logEvent.ThreadId);
                    Assert.IsTrue(logEvent.FilePath.Contains("LogTest.cs"));
                    Assert.AreEqual(37, logEvent.LineNumber);
                    Assert.AreEqual(moduleName, logEvent.ModluleName);
                });
        }

        /// <summary>Tests the debug level.</summary>
        [TestMethod]
        public void DebugTest() => TestLevel(EventLevel.Verbose, 1);

        /// <summary>Tests the info level.</summary>
        [TestMethod]
        public void InfoTest() => TestLevel(EventLevel.Informational, 2);

        /// <summary>Tests the warn level.</summary>
        [TestMethod]
        public void WarnTest() => TestLevel(EventLevel.Warning, 3);

        /// <summary>Tests the error level.</summary>
        [TestMethod]
        public void ErrorTest() => TestLevel(EventLevel.Error, 4);

        /// <summary>Tests the critical level.</summary>
        [TestMethod]
        public void CriticalTest() => TestLevel(EventLevel.Critical, 5);

        /// <summary>Tests the debug level with specified module name.</summary>
        [TestMethod]
        public void DebugTestModule() => TestLevel("Lawo", EventLevel.Verbose, 1);

        /// <summary>Tests the info level with specified module name.</summary>
        [TestMethod]
        public void InfoTestModule() => TestLevel("Lawo", EventLevel.Informational, 2);

        /// <summary>Tests the warn level with specified module name.</summary>
        [TestMethod]
        public void WarnTestModule() => TestLevel("Lawo", EventLevel.Warning, 3);

        /// <summary>Tests the error level with specified module name.</summary>
        [TestMethod]
        public void ErrorTestModule() => TestLevel("Lawo", EventLevel.Error, 4);

        /// <summary>Tests the critical level with specified module name.</summary>
        [TestMethod]
        public void CriticalTestModule() => TestLevel("Lawo", EventLevel.Critical, 5);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static void LogOnEachLevel(string testName)
        {
            // log on each level.
            Log.Critical(CreateLogMessage(testName, EventLevel.Critical));
            Log.Error(CreateLogMessage(testName, EventLevel.Error));
            Log.Warn(CreateLogMessage(testName, EventLevel.Warning));
            Log.Info(CreateLogMessage(testName, EventLevel.Informational));
            Log.Debug(CreateLogMessage(testName, EventLevel.Verbose));
        }

        private static void LogOnEachLevel(string moduleName, string testName)
        {
            // log on each level  with specified module name.
            Log.Critical(CreateLogMessage(testName, EventLevel.Critical), moduleName);
            Log.Error(CreateLogMessage(testName, EventLevel.Error), moduleName);
            Log.Warn(CreateLogMessage(testName, EventLevel.Warning), moduleName);
            Log.Info(CreateLogMessage(testName, EventLevel.Informational), moduleName);
            Log.Debug(CreateLogMessage(testName, EventLevel.Verbose), moduleName);
        }

        private static string CreateLogMessage(string testName, EventLevel eventLevel) => testName + "-" + eventLevel;

        private static async void TestLevel(EventLevel eventLevel, int expectedId, [CallerMemberName] string testName = null)
        {
            await Task.Run(async () =>
                {
                    // register for events of this level and higher (e.g. for warning level events of type warning, error and critical are expected).
                    TestLogListener listener = new TestLogListener(eventLevel);

                    LogOnEachLevel(testName);

                    var expectedLogMessage = CreateLogMessage(testName, eventLevel);
                    var logEvent = await WaitForLogEventAsync(listener.LogEvents, expectedLogMessage);

                    // check the log event.
                    Assert.AreEqual(eventLevel, logEvent.EventLevel);
                    Assert.AreEqual(expectedId, logEvent.EventId);
                    Assert.IsTrue(logEvent.LogMessage.Contains(expectedLogMessage));
                    Assert.AreEqual(string.Empty, logEvent.ModluleName);
                });
        }

        private static async void TestLevel(string moduleName, EventLevel eventLevel, int expectedId, [CallerMemberName] string testName = null)
        {
            await Task.Run(async () =>
            {
                // register for events of this level and higher (e.g. for warning level events of type warning, error and critical are expected).
                TestLogListener listener = new TestLogListener(eventLevel);

                LogOnEachLevel(moduleName, testName);

                var expectedLogMessage = CreateLogMessage(testName, eventLevel);
                var logEvent = await WaitForLogEventAsync(listener.LogEvents, expectedLogMessage);

                // check the log event.
                Assert.AreEqual(eventLevel, logEvent.EventLevel);
                Assert.AreEqual(expectedId, logEvent.EventId);
                Assert.IsTrue(logEvent.LogMessage.Contains(expectedLogMessage));
                Assert.AreEqual(moduleName, logEvent.ModluleName);
            });
        }

        private static async Task<TestLogListener.LogEvent> WaitForLogEventAsync(
            ObservableCollection<TestLogListener.LogEvent> collection, string logMessage)
        {
            TestLogListener.LogEvent foundLogEvent;

            while (((foundLogEvent = collection.FirstOrDefault(n => n.LogMessage == logMessage)) == null) &&
                await WaitForChangeAsync(collection.GetProperty(c => c.Count)) > 0)
            {
            }

            return foundLogEvent;
        }
    }
}
