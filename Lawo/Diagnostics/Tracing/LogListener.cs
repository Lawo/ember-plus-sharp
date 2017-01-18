////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.Diagnostics.Tracing
{
    using System.Diagnostics.Tracing;

    /// <summary>Log listener base class.</summary>
    /// <threadsafety static="true" instance="false"/>
    public abstract class LogListener : EventListener
    {
        /// <summary>Initializes a new instance of the <see cref="LogListener" /> class.</summary>
        /// <param name="eventLevel">The event level.</param>
        protected LogListener(EventLevel eventLevel)
        {
            this.eventLevel = eventLevel;
        }

        /// <summary>Called for all existing event sources when the event listener is created and when a new event
        /// source is attached to the listener. Enables log sources to the appropriate event level.</summary>
        /// <param name="eventSource">The event source.</param>
        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            var log = eventSource as Log;

            if (log != null)
            {
                this.EnableEvents(log, this.eventLevel);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private readonly EventLevel eventLevel;
    }
}
