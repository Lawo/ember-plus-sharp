////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.Diagnostics.Tracing
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.Tracing;
    using System.Globalization;

    /// <summary>Console event listener listens to log events and writes them to the console in debug mode.</summary>
    /// <threadsafety static="true" instance="false"/>
    public sealed class ConsoleLogListener : LogListener
    {
        /// <summary>
        /// Initializes the ConsoleLogListener.
        /// </summary>
        /// <param name="eventLevel">The event level.</param>
        public static void Initialize(EventLevel eventLevel)
        {
            if (instance == null)
            {
                instance = new ConsoleLogListener(eventLevel);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Called whenever an event has been written by an event source for which the event listener has enabled events.
        /// </summary>
        /// <param name="eventData">The event data.</param>
        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            if (eventData != null)
            {
#if DEBUG
                const string Format = "{0:yyyy-MM-dd HH\\:mm\\:ss\\.fff} {1, -13} [{3}] {6} : {2}";
                var p = eventData.Payload;
                var newFormatedLine = string.Format(
                    CultureInfo.InvariantCulture, Format, DateTime.Now, eventData.Level, p[0], p[1], p[2], p[3], p[4]);
                Debug.WriteLine(newFormatedLine);
#endif
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static ConsoleLogListener instance;

        private ConsoleLogListener(EventLevel eventLevel)
            : base(eventLevel)
        {
        }
    }
}
