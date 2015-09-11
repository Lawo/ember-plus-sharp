////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.IO
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>Transparently encodes and decodes the communication with a telnet server as defined in
    /// <a href="http://tools.ietf.org/html/rfc854">RFC 854</a>.</summary>
    /// <remarks>
    /// <para>A call to <see cref="ReadAsync"/> removes data from the internal read buffer. If the internal buffer is
    /// empty, it is filled first by invoking <see cref="ReadAsyncCallback"/>. The data is then decoded and the decoded
    /// form is then returned.</para>
    /// <para>A call to <see cref="WriteAsync"/> encodes the passed data and then appends the encoded form to the
    /// internal write buffer, while flushing as needed.</para>
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    [CLSCompliant(false)]
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Type derives from Stream, CA bug?")]
    public sealed class TelnetStream : BufferStream
    {
        private readonly Func<bool> dataAvailable;
        private ReadState readState;
        private byte readCommand;
        private bool previousWasEscapeByte;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Initializes a new instance of the <see cref="TelnetStream"/> class.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="readAsync"/>, <paramref name="writeAsync"/> and/or
        /// <paramref name="dataAvailable"/> equal <c>null</c>.</exception>
        public TelnetStream(
            ReadAsyncCallback readAsync, WriteAsyncCallback writeAsync, Func<bool> dataAvailable) : base(
            new ReadBuffer(readAsync, Defaults.PhysicalStreamBufferSize),
            new WriteBuffer(writeAsync, Defaults.PhysicalStreamBufferSize))
        {
            if (readAsync == null)
            {
                throw new ArgumentNullException("readAsync");
            }

            if (writeAsync == null)
            {
                throw new ArgumentNullException("writeAsync");
            }

            if (dataAvailable == null)
            {
                throw new ArgumentNullException("dataAvailable");
            }

            this.dataAvailable = dataAvailable;
        }

        /// <summary>See <see cref="Stream.ReadAsync(byte[], int, int, CancellationToken)"/>.</summary>
        public sealed override async Task<int> ReadAsync(
            byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var readBuffer = this.ReadBuffer;
            var index = offset;
            var pastEnd = offset + count;

            // (index == offset) ensures that we only try to get more encoded data if we haven't yet copied anything
            // into buffer
            while ((index < pastEnd) && ((readBuffer.Index < readBuffer.Count) ||
                ((index == offset) && await readBuffer.ReadAsync(cancellationToken))))
            {
                var response = this.ReadByte(readBuffer, buffer, ref index);

                if (response != null)
                {
                    // TODO: The following is dangerous as another WriteAsync call could still be pending. It's probably
                    // best to use something like TaskQueue internally.
                    await this.WriteBuffer.WriteAsync(response, 0, response.Length, cancellationToken);
                    await this.WriteBuffer.FlushAsync(cancellationToken);
                }
            }

            return index - offset;
        }

        /// <summary>See <see cref="Stream.WriteAsync(byte[], int, int, CancellationToken)"/>.</summary>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated with call to BufferHelper.AssertValidRange.")]
        public sealed override async Task WriteAsync(
            byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var writeBuffer = this.WriteBuffer;
            var pastEnd = offset + count;

            while ((offset < pastEnd) && ((writeBuffer.Count < writeBuffer.Capacity) ||
                await writeBuffer.FlushAsync(cancellationToken)))
            {
                offset = WriteByte(buffer, offset, writeBuffer);
            }
        }

        /// <summary>See <see cref="NonSeekableStream.FlushAsync"/>.</summary>
        public sealed override async Task FlushAsync(CancellationToken cancellationToken)
        {
            await this.WriteBuffer.FlushAsync(cancellationToken);
            await base.FlushAsync(cancellationToken);
        }

        /// <summary>Gets a value indicating whether data is available on the <see cref="TelnetStream"/> to be
        /// read.</summary>
        public bool DataAvailable
        {
            get { return (this.ReadBuffer.Index < this.ReadBuffer.Count) || this.dataAvailable(); }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private byte[] ReadByte(ReadBuffer readBuffer, byte[] buffer, ref int index)
        {
            var currentByte = readBuffer[readBuffer.Index++];

            switch (this.readState)
            {
                case ReadState.Data:
                    if (currentByte == Command.InterpretAsCommand)
                    {
                        this.readState = ReadState.Command;
                    }
                    else
                    {
                        buffer[index++] = currentByte;
                    }

                    return null;
                case ReadState.Command:
                    switch (currentByte)
                    {
                        case Command.Will:
                        case Command.Wont:
                        case Command.Do:
                        case Command.Dont:
                            this.readState = ReadState.OptionCode;
                            this.readCommand = currentByte;
                            break;
                        case Command.InterpretAsCommand:
                            this.readState = ReadState.Data;
                            buffer[index++] = currentByte;
                            break;
                        default:
                            this.readState = ReadState.Data;
                            break;
                    }

                    return null;
                default:
                    this.readState = ReadState.Data;
                    byte response;

                    if (currentByte == Option.SuppressGoAhead)
                    {
                        response = readCommand == Command.Do ? Command.Will : Command.Do;
                    }
                    else
                    {
                        response = readCommand == Command.Do ? Command.Wont : Command.Dont;
                    }

                    return new[] { Command.InterpretAsCommand, response, currentByte };
            }
        }

        private int WriteByte(byte[] buffer, int offset, WriteBuffer writeBuffer)
        {
            var currentByte = buffer[offset];

            if (this.previousWasEscapeByte)
            {
                this.previousWasEscapeByte = false;
                ++offset;
            }
            else
            {
                if (currentByte == Command.InterpretAsCommand)
                {
                    this.previousWasEscapeByte = true;
                }
                else
                {
                    ++offset;
                }
            }

            writeBuffer[writeBuffer.Count++] = currentByte;
            return offset;
        }

        /// <summary>Enumerates the supported Telnet commands.</summary>
        private static class Command
        {
            internal const byte Will = 251;
            internal const byte Wont = 252;
            internal const byte Do = 253;
            internal const byte Dont = 254;
            internal const byte InterpretAsCommand = 255;
        }

        /// <summary>Enumerates the supported options.</summary>
        private static class Option
        {
            internal const byte SuppressGoAhead = 3;
        }

        /// <summary>Enumerates the read states.</summary>
        private enum ReadState
        {
            /// <summary>The previous byte was a data byte or the last byte of a command.</summary>
            Data,

            /// <summary>The previous byte was a IAC byte.</summary>
            Command,

            /// <summary>The previous byte was a command that requires an option code.</summary>
            OptionCode
        }
    }
}
