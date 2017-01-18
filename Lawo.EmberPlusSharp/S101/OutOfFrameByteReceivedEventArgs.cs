////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.S101
{
    using System;

    /// <summary>Provides the data for the <see cref="S101Reader.OutOfFrameByteReceived"/> and
    /// <see cref="S101Client.OutOfFrameByteReceived"/> events.</summary>
    /// <threadsafety static="true" instance="false"/>
    public sealed class OutOfFrameByteReceivedEventArgs : EventArgs
    {
        /// <summary>Gets the message.</summary>
        public byte Value { get; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal OutOfFrameByteReceivedEventArgs(byte value)
        {
            this.Value = value;
        }
    }
}