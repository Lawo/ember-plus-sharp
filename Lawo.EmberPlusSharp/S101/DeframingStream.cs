////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.S101
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    using IO;

    /// <summary>Transparently decodes a single frame.</summary>
    /// <remarks>A call to any of the Read methods of this stream removes data from <see cref="ReadBuffer"/>
    /// object passed to the constructor. The data is then decoded and the decoded form is then returned.</remarks>
    /// <threadsafety static="true" instance="false"/>
    internal sealed class DeframingStream : BufferStream
    {
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated with call to BufferHelper.AssertValidRange.")]
        public sealed override async Task<int> ReadAsync(
            byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (this.state != State.AfterFrame)
            {
                // We're reading the full frame before touching the passed buffer, so that we have a chance to silently
                // throw away data if it turns out to be invalid. This is necessary so that we can correctly
                // communicate with consumers and providers which mix Ember+ data with other data. For example,
                // vsmStudio often sends a first Ember message followed by an Ember+ message.
                var readBuffer = this.ReadBuffer;

                while (((readBuffer.Index < readBuffer.Count) || await readBuffer.ReadAsync(cancellationToken)) &&
                    this.ReadByte(readBuffer))
                {
                }
            }

            var index = offset;
            var pastEnd = offset + count;

            // The last 2 bytes are the CRC
            while ((index < pastEnd) && (this.decodedQueue.Count > 2))
            {
                buffer[index++] = this.decodedQueue.Dequeue();
            }

            return index - offset;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Initializes a new instance of the <see cref="DeframingStream"/> class.</summary>
        internal DeframingStream(ReadBuffer readBuffer, Action<byte> outOfFrameByteReceived)
            : base(readBuffer, null)
        {
            this.outOfFrameByteReceived = outOfFrameByteReceived;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private readonly Queue<byte> decodedQueue = new Queue<byte>(64);
        private readonly Action<byte> outOfFrameByteReceived;
        private State state;
        private ushort crc = 0xFFFF;

        /// <summary>Enumerates the decoding states.</summary>
        private enum State
        {
            /// <summary>The start state.</summary>
            BeforeFrame,

            /// <summary>The previous byte was either a BOF or a normal byte.</summary>
            InFrame,

            /// <summary>The previous byte was the escape byte.</summary>
            InFrameEscaped,

            /// <summary>The previous byte was either the EOF or unexpected (an exception was thrown).</summary>
            AfterFrame
        }

        private bool ReadByte(ReadBuffer readBuffer)
        {
            var currentByte = readBuffer[readBuffer.Index++];

            switch (this.state)
            {
                case State.BeforeFrame:
                    if (currentByte == Frame.BeginOfFrame)
                    {
                        this.state = State.InFrame;
                    }
                    else
                    {
                        this.outOfFrameByteReceived(currentByte);
                    }

                    break;
                case State.InFrame:
                    if (currentByte < Frame.InvalidStart)
                    {
                        this.crc = Crc.AddCrcCcitt(this.crc, currentByte);
                        this.decodedQueue.Enqueue(currentByte);
                    }
                    else
                    {
                        switch (currentByte)
                        {
                            case Frame.EscapeByte:
                                this.state = State.InFrameEscaped;
                                break;
                            case Frame.BeginOfFrame:
                                this.decodedQueue.Clear();
                                this.crc = 0xFFFF;
                                break;
                            case Frame.EndOfFrame:
                                this.state = State.AfterFrame;

                                if (this.crc != 0xF0B8)
                                {
                                    this.decodedQueue.Clear();
                                }

                                return false;
                            default:
                                this.state = State.AfterFrame;
                                this.decodedQueue.Clear();
                                break;
                        }
                    }

                    break;
                case State.InFrameEscaped:
                    if (currentByte >= Frame.InvalidStart)
                    {
                        this.state = State.AfterFrame;
                        this.decodedQueue.Clear();
                    }

                    currentByte = (byte)(currentByte ^ Frame.EscapeXor);
                    this.crc = Crc.AddCrcCcitt(this.crc, currentByte);
                    this.decodedQueue.Enqueue(currentByte);
                    this.state = State.InFrame;
                    break;
            }

            return true;
        }
    }
}
