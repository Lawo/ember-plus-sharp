////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.Diagnostics.Tracing
{
    using System.Collections.ObjectModel;
    using System.Diagnostics.Tracing;

    internal sealed class TestLogListener : LogListener
    {
        public TestLogListener(EventLevel eventLevel)
            : base(eventLevel)
        {
            this.LogEvents = new ObservableCollection<LogEvent>();
        }

        public ObservableCollection<LogEvent> LogEvents { get; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal class LogEvent
        {
            internal EventLevel EventLevel { get; set; }

            internal int EventId { get; set; }

            internal string LogMessage { get; set; }

            internal uint ThreadId { get; set; }

            internal string FilePath { get; set; }

            internal int LineNumber { get; set; }

            internal string ModluleName { get; set; }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            if (eventData != null)
            {
                var logEvent = new LogEvent();

                logEvent.EventLevel = eventData.Level;
                logEvent.EventId = eventData.EventId;

                var payload = eventData.Payload;

                logEvent.LogMessage = payload[0] as string;
                logEvent.ThreadId = (uint)payload[1];
                logEvent.FilePath = payload[2] as string;
                logEvent.LineNumber = (int)payload[3];
                logEvent.ModluleName = payload[4] as string;

                this.LogEvents.Add(logEvent);
            }
        }
    }
}
