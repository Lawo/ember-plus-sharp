////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.IO
{
    using System;

    /// <summary>Provides the data for the <see cref="IMonitoredConnection.ConnectionLost"/> event.</summary>
    /// <threadsafety static="true" instance="false"/>
    public sealed class ConnectionLostEventArgs : EventArgs
    {
        /// <summary>Initializes a new instance of the <see cref="ConnectionLostEventArgs"/> class.</summary>
        public ConnectionLostEventArgs(Exception exception)
        {
            this.Exception = exception;
        }

        /// <summary>Gets the exception that was thrown when the connection was lost.</summary>
        public Exception Exception { get; }
    }
}