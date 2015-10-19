////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.S101
{
    using System;

    /// <summary>Provides the data for the <see cref="S101Client.EmberDataReceived"/> event.</summary>
    /// <threadsafety static="true" instance="false"/>
    public sealed class MessageReceivedEventArgs : EventArgs
    {
        private readonly S101Message message;
        private readonly byte[] payload;
        private readonly bool isAnotherMessageAvailable;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets the message.</summary>
        public S101Message Message
        {
            get { return this.message; }
        }

        /// <summary>Gets a value indicating whether another message is available.</summary>
        public bool IsAnotherMessageAvailable
        {
            get { return this.isAnotherMessageAvailable; }
        }

        /// <summary>Gets the payload of the message.</summary>
        /// <returns>The payload of the message if the message has a payload; otherwise <c>null</c>.</returns>
        public byte[] GetPayload()
        {
            return this.payload;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal MessageReceivedEventArgs(S101Message message, byte[] payload, bool isAnotherMessageAvailable)
        {
            this.message = message;
            this.payload = payload;
            this.isAnotherMessageAvailable = isAnotherMessageAvailable;
        }
    }
}