////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.S101
{
    using System;

    /// <summary>Represents a logger for S101 activity.</summary>
    public interface IS101Logger : IDisposable
    {
        /// <summary>Logs an event by calling
        /// <see cref="LogEvent(string, string)">LogEvent(<paramref name="eventName"/>, null)</see>.</summary>
        EventInfo LogEvent(string eventName);

        /// <summary>Logs an event with string data.</summary>
        /// <param name="eventName">The name of the event to log.</param>
        /// <param name="data">The data to log.</param>
        /// <exception cref="ObjectDisposedException"><see cref="IDisposable.Dispose"/> has been called.</exception>
        /// <remarks>An event can be anything except a message or an exception.</remarks>
        EventInfo LogEvent(string eventName, string data);

        /// <summary>Logs data.</summary>
        /// <param name="type">The type of the data to log.</param>
        /// <param name="direction">The direction of the data, usually either <c>"Send"</c> or <c>"Receive"</c>.
        /// </param>
        /// <param name="buffer">The buffer containing the data to log.</param>
        /// <param name="index">The position in the buffer indicating the start of the data to log.</param>
        /// <param name="count">The number of bytes to log.</param>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> equals <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><list type="bullet">
        /// <item><paramref name="index"/> or <paramref name="count"/> is less than zero, or</item>
        /// <item><paramref name="buffer"/>.Length minus <paramref name="index"/> is less
        /// than <paramref name="count"/>.</item>
        /// </list></exception>
        /// <exception cref="ObjectDisposedException"><see cref="IDisposable.Dispose"/> has been called.</exception>
        EventInfo LogData(string type, string direction, byte[] buffer, int index, int count);

        /// <summary>Logs a message.</summary>
        /// <param name="direction">The direction of the message, usually either <c>"Send"</c> or <c>"Receive"</c>.
        /// </param>
        /// <param name="message">The message to log.</param>
        /// <param name="payload">The payload of the message if the message has a payload; otherwise <c>null</c>.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="message"/> and/or <paramref name="direction"/>
        /// equal <c>null</c>.
        /// </exception>
        /// <exception cref="ObjectDisposedException"><see cref="IDisposable.Dispose"/> has been called.</exception>
        EventInfo LogMessage(string direction, S101Message message, byte[] payload);

        /// <summary>Logs <paramref name="exception"/>.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="exception"/> equals <c>null</c>.</exception>
        /// <exception cref="ObjectDisposedException"><see cref="IDisposable.Dispose"/> has been called.</exception>
        EventInfo LogException(string direction, Exception exception);
    }
}
