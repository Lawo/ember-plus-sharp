////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2016 Lawo AG (http://www.lawo.com).</copyright>
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
        private readonly Action<byte> outOfFrameByteReceived;
        private State state;
        private ushort crc = 0xFFFF;
        private readonly Queue<byte> decodedQueue = new Queue<byte>();

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated with call to BufferHelper.AssertValidRange.")]
        public sealed override async Task<int> ReadAsync(
            byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (this.state == State.AfterFrame)
            {
                return 0;
            }

            var readBuffer = this.ReadBuffer;
            var index = offset;
            var pastEnd = offset + count;

            // (index == offset) ensures that we only try to get more encoded data if we haven't yet copied anything
            // into buffer
            while ((index < pastEnd) && ((readBuffer.Index < readBuffer.Count) ||
                ((index == offset) && await readBuffer.ReadAsync(cancellationToken))))
            {
                if (!this.ReadByte(readBuffer, buffer, ref index))
                {
                    break;
                }
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

        private bool ReadByte(ReadBuffer readBuffer, byte[] buffer, ref int index)
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
                            case Frame.EndOfFrame:
                                this.state = State.AfterFrame;

                                if (this.crc != 0xF0B8)
                                {
                                    throw new S101Exception("CRC failed.");
                                }

                                return false;
                            default:
                                this.state = State.AfterFrame;
                                throw new S101Exception("Invalid byte in frame.");
                        }
                    }

                    break;
                case State.InFrameEscaped:
                    if (currentByte >= Frame.InvalidStart)
                    {
                        this.state = State.AfterFrame;
                        throw new S101Exception("Invalid escaped byte.");
                    }

                    currentByte = (byte)(currentByte ^ Frame.EscapeXor);
                    this.crc = Crc.AddCrcCcitt(this.crc, currentByte);
                    this.decodedQueue.Enqueue(currentByte);
                    this.state = State.InFrame;
                    break;
            }

            if (this.decodedQueue.Count == 3)
            {
                buffer[index++] = this.decodedQueue.Dequeue();
            }

            return true;
        }
    }
}
