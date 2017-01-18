////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.S101
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using IO;

    /// <summary>Transparently encodes a single message.</summary>
    /// <remarks>
    /// <para>At construction, the passed message is first appended to the <see cref="WriteBuffer"/> object passed to
    /// <see cref="CreateAsync"/>. Afterwards, when data is written to this stream then it is first encoded and the
    /// encoded form is then appended to the <see cref="WriteBuffer"/> object.</para>
    /// <para><b>Caution</b>: <see cref="DisposeAsync"/> <b>must</b> be called in the end.</para>
    /// <para>If necessary, the message plus payload is automatically partitioned into multiple packets such that the
    /// unencoded length of each packet does not exceed 1024 bytes.</para>
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    internal sealed class MessageEncodingStream : NonSeekableStream
    {
        public sealed override bool CanWrite => this.framingStream != null;

        public sealed override async Task DisposeAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (this.framingStream != null)
                {
                    if (this.message.CanHaveMultiplePackets)
                    {
                        await this.DisposeAndCreateFramingStreamAsync(
                            PacketFlags.EmptyPacket | PacketFlags.LastPacket, cancellationToken);
                    }

                    await this.DisposeFramingStream(cancellationToken);
                    await this.rawBuffer.FlushAsync(cancellationToken);
                    await base.DisposeAsync(cancellationToken);
                }
            }
            finally
            {
                this.framingStream = null;
            }
        }

        public sealed override async Task WriteAsync(
            byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            this.AssertNotDisposed();
            BufferHelper.AssertValidRange(buffer, "buffer", offset, "offset", count, "count");

            while (count > 0)
            {
                if (this.framingStream.TotalCount >= MaxFrameLength)
                {
                    await this.DisposeAndCreateFramingStreamAsync(PacketFlags.None, cancellationToken);
                }

                var countToWrite = Math.Min(count, MaxFrameLength - this.framingStream.TotalCount);
                await this.unframedBuffer.WriteAsync(buffer, offset, countToWrite, cancellationToken);
                offset += countToWrite;
                count -= countToWrite;
            }
        }

        public sealed override async Task FlushAsync(CancellationToken cancellationToken)
        {
            this.AssertNotDisposed();
            await this.unframedBuffer.FlushAsync(cancellationToken);
            await this.framingStream.FlushAsync(cancellationToken);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal static async Task<MessageEncodingStream> CreateAsync(
            WriteBuffer rawBuffer, S101Message message, CancellationToken cancellationToken)
        {
            message.PacketFlags =
                PacketFlags.FirstPacket | (message.CanHaveMultiplePackets ? PacketFlags.None : PacketFlags.LastPacket);
            var framingStream = await FramingStream.CreateAsync(rawBuffer, cancellationToken);
            var result = new MessageEncodingStream(message, rawBuffer, framingStream);
            await message.WriteToAsync(result.unframedBuffer, cancellationToken);
            return result;
        }

        internal async Task WriteOutOfFrameByteAsync(byte value, CancellationToken cancellationToken)
        {
            await this.DisposeFramingStream(cancellationToken);
            await this.rawBuffer.ReserveAsync(1, cancellationToken);
            this.rawBuffer[this.rawBuffer.Count++] = value;
            await this.CreateFramingStream(PacketFlags.None, cancellationToken);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private const int MaxFrameLength = 1024;
        private readonly WriteBuffer unframedBuffer;
        private readonly S101Message message;
        private readonly WriteBuffer rawBuffer;
        private FramingStream framingStream;

        private MessageEncodingStream(S101Message message, WriteBuffer rawBuffer, FramingStream framingStream)
        {
            this.unframedBuffer = new WriteBuffer(this.WriteUnframedAsync, Constants.MessageHeaderMaxLength);
            this.message = message;
            this.rawBuffer = rawBuffer;
            this.framingStream = framingStream;
        }

        private async Task DisposeAndCreateFramingStreamAsync(PacketFlags packetFlags, CancellationToken cancellationToken)
        {
            await this.DisposeFramingStream(cancellationToken);
            await this.CreateFramingStream(packetFlags, cancellationToken);
        }

        private async Task DisposeFramingStream(CancellationToken cancellationToken)
        {
            await this.unframedBuffer.FlushAsync(cancellationToken);
            await this.framingStream.DisposeAsync(cancellationToken);
        }

        private async Task CreateFramingStream(PacketFlags packetFlags, CancellationToken cancellationToken)
        {
            this.framingStream = await FramingStream.CreateAsync(this.rawBuffer, cancellationToken);
            this.message.PacketFlags = packetFlags;
            await this.message.WriteToAsync(this.unframedBuffer, cancellationToken);
        }

        private Task WriteUnframedAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
            this.framingStream.WriteAsync(buffer, offset, count, cancellationToken);
    }
}
