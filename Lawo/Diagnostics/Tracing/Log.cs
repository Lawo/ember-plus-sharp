////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.Diagnostics.Tracing
{
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Tracing;
    using System.Runtime.CompilerServices;
    using Threading;

    /// <summary>Implements log functionality.</summary>
    /// <threadsafety static="true" instance="false"/>
    public sealed class Log : EventSource
    {
        /// <summary>Gets the instance.</summary>
        public static Log Instance { get; } = new Log();

        /// <summary>
        /// Logs on debug level.
        /// </summary>
        /// <param name="logMessage">The log message.</param>
        /// <param name="lineNumber">The line number in the source code at which the method is called. Set by compiler services.</param>
        /// <param name="filePath">The source file that contains the caller. Set by compiler services.</param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Default values set by CompilerServices.")]
        public static void Debug(
            string logMessage, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string filePath = null)
        {
            Instance.LogDebug(logMessage, NativeMethods.GetCurrentThreadId(), filePath, lineNumber, ModuleNameDefault);
        }

        /// <summary>
        /// Logs on debug level.
        /// </summary>
        /// <param name="logMessage">The log message.</param>
        /// <param name="moduleName">The name of the module that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source code at which the method is called. Set by compiler services.</param>
        /// <param name="filePath">The source file that contains the caller. Set by compiler services.</param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Default values set by CompilerServices.")]
        public static void Debug(
            string logMessage,
            string moduleName,
            [CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string filePath = null)
        {
            Instance.LogDebug(logMessage, NativeMethods.GetCurrentThreadId(), filePath, lineNumber, moduleName);
        }

        /// <summary>
        /// Logs on info level.
        /// </summary>
        /// <param name="logMessage">The log message.</param>
        /// <param name="lineNumber">The line number in the source code at which the method is called. Set by compiler services.</param>
        /// <param name="filePath">The source file that contains the caller. Set by compiler services.</param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Default values set by CompilerServices.")]
        public static void Info(
            string logMessage,
            [CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string filePath = null)
        {
            Instance.LogInfo(logMessage, NativeMethods.GetCurrentThreadId(), filePath, lineNumber, ModuleNameDefault);
        }

        /// <summary>
        /// Logs on info level.
        /// </summary>
        /// <param name="logMessage">The log message.</param>
        /// <param name="moduleName">The name of the module that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source code at which the method is called. Set by compiler services.</param>
        /// <param name="filePath">The source file that contains the caller. Set by compiler services.</param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Default values set by CompilerServices.")]
        public static void Info(
            string logMessage,
            string moduleName,
            [CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string filePath = null)
        {
            Instance.LogInfo(logMessage, NativeMethods.GetCurrentThreadId(), filePath, lineNumber, moduleName);
        }

        /// <summary>
        /// Logs on warning level.
        /// </summary>
        /// <param name="logMessage">The log message.</param>
        /// <param name="lineNumber">The line number in the source code at which the method is called. Set by compiler services.</param>
        /// <param name="filePath">The source file that contains the caller. Set by compiler services.</param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Default values set by CompilerServices.")]
        public static void Warn(
            string logMessage,
            [CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string filePath = null)
        {
            Instance.LogWarn(logMessage, NativeMethods.GetCurrentThreadId(), filePath, lineNumber, ModuleNameDefault);
        }

        /// <summary>
        /// Logs on warn level.
        /// </summary>
        /// <param name="logMessage">The log message.</param>
        /// <param name="moduleName">The name of the module that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source code at which the method is called. Set by compiler services.</param>
        /// <param name="filePath">The source file that contains the caller. Set by compiler services.</param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Default values set by CompilerServices.")]
        public static void Warn(
            string logMessage,
            string moduleName,
            [CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string filePath = null)
        {
            Instance.LogWarn(logMessage, NativeMethods.GetCurrentThreadId(), filePath, lineNumber, moduleName);
        }

        /// <summary>
        /// Logs on error level.
        /// </summary>
        /// <param name="logMessage">The log message.</param>
        /// <param name="lineNumber">The line number in the source code at which the method is called. Set by compiler services.</param>
        /// <param name="filePath">The source file that contains the caller. Set by compiler services.</param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Default values set by CompilerServices.")]
        public static void Error(
            string logMessage,
            [CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string filePath = null)
        {
            Instance.LogError(logMessage, NativeMethods.GetCurrentThreadId(), filePath, lineNumber, ModuleNameDefault);
        }

        /// <summary>
        /// Logs on error level.
        /// </summary>
        /// <param name="logMessage">The log message.</param>
        /// <param name="moduleName">The name of the module that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source code at which the method is called. Set by compiler services.</param>
        /// <param name="filePath">The source file that contains the caller. Set by compiler services.</param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Default values set by CompilerServices.")]
        public static void Error(
            string logMessage,
            string moduleName,
            [CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string filePath = null)
        {
            Instance.LogError(logMessage, NativeMethods.GetCurrentThreadId(), filePath, lineNumber, moduleName);
        }

        /// <summary>
        /// Logs on critical level.
        /// </summary>
        /// <param name="logMessage">The log message.</param>
        /// <param name="lineNumber">The line number in the source code at which the method is called. Set by compiler services.</param>
        /// <param name="filePath">The source file that contains the caller. Set by compiler services.</param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Default values set by CompilerServices.")]
        public static void Critical(
            string logMessage,
            [CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string filePath = null)
        {
            Instance.LogCritical(logMessage, NativeMethods.GetCurrentThreadId(), filePath, lineNumber, ModuleNameDefault);
        }

        /// <summary>
        /// Logs on critical level.
        /// </summary>
        /// <param name="logMessage">The log message.</param>
        /// <param name="moduleName">The name of the module that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source code at which the method is called. Set by compiler services.</param>
        /// <param name="filePath">The source file that contains the caller. Set by compiler services.</param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Default values set by CompilerServices.")]
        public static void Critical(
            string logMessage,
            string moduleName,
            [CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string filePath = null)
        {
            Instance.LogCritical(logMessage, NativeMethods.GetCurrentThreadId(), filePath, lineNumber, moduleName);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static readonly string ModuleNameDefault = string.Empty;

        private Log()
        {
        }

        [Event(1, Level = EventLevel.Verbose)]
        private void LogDebug(string logMessage, uint threadId, string filePath, int lineNumber, string moduleName) =>
            this.WriteEvent(1, logMessage, threadId, filePath, lineNumber, moduleName);

        [Event(2, Level = EventLevel.Informational)]
        private void LogInfo(string logMessage, uint threadId, string filePath, int lineNumber, string moduleName) =>
            this.WriteEvent(2, logMessage, threadId, filePath, lineNumber, moduleName);

        [Event(3, Level = EventLevel.Warning)]
        private void LogWarn(string logMessage, uint threadId, string filePath, int lineNumber, string moduleName) =>
            this.WriteEvent(3, logMessage, threadId, filePath, lineNumber, moduleName);

        [Event(4, Level = EventLevel.Error)]
        private void LogError(string logMessage, uint threadId, string filePath, int lineNumber, string moduleName) =>
            this.WriteEvent(4, logMessage, threadId, filePath, lineNumber, moduleName);

        [Event(5, Level = EventLevel.Critical)]
        private void LogCritical(string logMessage, uint threadId, string filePath, int lineNumber, string moduleName)
        {
            this.WriteEvent(5, logMessage, threadId, filePath, lineNumber, moduleName);
        }
    }
}
