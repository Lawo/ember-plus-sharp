////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.Diagnostics.Tracing
{
    using System.Diagnostics.Tracing;

    /// <summary>Console event listener listens to log events and writes them to the console in debug mode.</summary>
    /// <threadsafety static="true" instance="false"/>
    public sealed class ConsoleLogListener : LogListener
    {
        private static ConsoleLogListener instance;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes the ConsoleLogListener.
        /// </summary>
        /// <param name="eventLevel">The event level.</param>
        public static void Initialize(EventLevel eventLevel)
        {
            if (null == instance)
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
                var payload = eventData.Payload;
                var newFormatedLine = string.Format(CultureInfo.InvariantCulture, Format, DateTime.Now, eventData.Level, payload[0], payload[1], payload[2], payload[3], payload[4]);

                Debug.WriteLine(newFormatedLine);
#endif
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private ConsoleLogListener(EventLevel eventLevel)
            : base(eventLevel)
        {
        }
    }
}
