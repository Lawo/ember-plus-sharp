////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.S101
{
    using System;

    /// <summary>Provides the data for the <see cref="S101Client.EmberDataReceived"/> event.</summary>
    /// <threadsafety static="true" instance="false"/>
    public sealed class MessageReceivedEventArgs : EventArgs
    {
        /// <summary>Gets the message.</summary>
        public S101Message Message => this.message;

        /// <summary>Gets a value indicating whether another message is available.</summary>
        public bool IsAnotherMessageAvailable => this.isAnotherMessageAvailable;

        /// <summary>Gets the payload of the message.</summary>
        /// <returns>The payload of the message if the message has a payload; otherwise <c>null</c>.</returns>
        public byte[] GetPayload() => this.payload;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal MessageReceivedEventArgs(S101Message message, byte[] payload, bool isAnotherMessageAvailable)
        {
            this.message = message;
            this.payload = payload;
            this.isAnotherMessageAvailable = isAnotherMessageAvailable;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private readonly S101Message message;
        private readonly byte[] payload;
        private readonly bool isAnotherMessageAvailable;
    }
}