////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.Diagnostics.Tracing
{
    using System.Diagnostics.Tracing;

    /// <summary>Log listener base class.</summary>
    /// <threadsafety static="true" instance="false"/>
    public abstract class LogListener : EventListener
    {
        private EventLevel eventLevel;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <see cref="LogListener" /> class.
        /// </summary>
        /// <param name="eventLevel">The event level.</param>
        protected LogListener(EventLevel eventLevel)
        {
            this.eventLevel = eventLevel;
        }

        /// <summary>
        /// Called for all existing event sources when the event listener is created and when a new event source is attached to the listener.
        /// Enables log sources to the appropriate event level.
        /// </summary>
        /// <param name="eventSource">The event source.</param>
        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            var log = eventSource as Log;

            if (null != log)
            {
                this.EnableEvents(log, this.eventLevel);
            }
        }
    }
}
