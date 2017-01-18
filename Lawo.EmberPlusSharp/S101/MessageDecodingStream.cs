////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.S101
{
    using System;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;

    using IO;

    /// <summary>Transparently decodes a single S101 message.</summary>
    /// <remarks>
    /// <para>At construction, a <see cref="Message"/> object is first decoded from the <see cref="ReadBuffer"/> object
    /// passed to <see cref="CreateAsync"/> and made available through the <see cref="Message"/> property. Afterwards, a
    /// call to any of the Read methods of this stream removes data from <see cref="ReadBuffer"/> object passed to the
    /// <see cref="CreateAsync"/>. The data is then decoded and the decoded form is then returned.</para>
    /// <para>If a message contains multiple packets, their payload is automatically joined such that it can be read
    /// through this stream as if the message consisted of only one packet.</para>
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    internal sealed class MessageDecodingStream : NonSeekableStream
    {
        public sealed override bool CanRead => this.deframingStream != null;

        public sealed override async Task DisposeAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (this.deframingStream != null)
                {
                    while (await this.ReadAsync(
                        this.discardBuffer, 0, this.discardBuffer.Length, cancellationToken) > 0)
                    {
                    }

                    await this.deframingStream.DisposeAsync(cancellationToken);
                    await base.DisposeAsync(cancellationToken);
                }
            }
            finally
            {
                this.deframingStream = null;
            }
        }

        public sealed override Task<int> ReadAsync(
            byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            this.AssertNotDisposed();
            BufferHelper.AssertValidRange(buffer, "buffer", offset, "offset", count, "count");
            return StreamHelper.TryFillAsync(this.ReadFromCurrentPacketAsync, buffer, offset, count, cancellationToken);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal static async Task<MessageDecodingStream> CreateAsync(
            ReadBuffer rawBuffer,
            byte[] discardBuffer,
            Action<byte> outOfFrameByteReceived,
            CancellationToken cancellationToken)
        {
            var result = new MessageDecodingStream(rawBuffer, discardBuffer, outOfFrameByteReceived);
            var newMessage = await S101Message.ReadFromAsync(result.deframedBuffer, cancellationToken);

            if ((newMessage != null) && newMessage.CanHaveMultiplePackets &&
                ((newMessage.PacketFlags & PacketFlags.FirstPacket) == 0))
            {
                throw new S101Exception(string.Format(
                    CultureInfo.InvariantCulture, "Missing {0} flag in first packet.", PacketFlags.FirstPacket));
            }

            result.message = newMessage;
            return result;
        }

        internal S101Message Message => this.message;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private readonly ReadBuffer rawBuffer;
        private readonly byte[] discardBuffer;
        private readonly Action<byte> outOfFrameByteReceived;
        private readonly ReadBuffer deframedBuffer;
        private DeframingStream deframingStream;
        private S101Message message;

        private MessageDecodingStream(ReadBuffer rawBuffer, byte[] discardBuffer, Action<byte> outOfFrameByteReceived)
        {
            this.rawBuffer = rawBuffer;
            this.discardBuffer = discardBuffer;
            this.outOfFrameByteReceived = outOfFrameByteReceived;

            // This buffer is kept small in size, because a new one is allocated for each message.
            // This has the effect that only the bytes of reads <= MessageHeaderMaxLength bytes are actually copied into
            // this buffer. Larger reads are automatically done by calling this.ReadDeframed (without copying the bytes
            // into the MessageHeaderMaxLength byte buffer first). The former happens when packet headers are read
            // (multiple small-sized reads), the latter happens when the payload is read (typically done with a buffer
            // >= 1024 bytes).
            // This approach minimizes the allocations per message, while guaranteeing the best possible performance for
            // header *and* payload reading.
            this.deframedBuffer = new ReadBuffer(this.ReadDeframedAsync, Constants.MessageHeaderMaxLength);
            this.deframingStream = new DeframingStream(this.rawBuffer, this.outOfFrameByteReceived);
        }

        private async Task<int> ReadFromCurrentPacketAsync(
            byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            int read;

            while (((read = await this.deframedBuffer.ReadAsync(buffer, offset, count, cancellationToken)) == 0) &&
                (count > 0) && (this.message != null) && this.message.CanHavePayload &&
                this.message.CanHaveMultiplePackets && ((this.message.PacketFlags & PacketFlags.LastPacket) == 0))
            {
                this.deframingStream.Dispose();
                this.deframingStream = new DeframingStream(this.rawBuffer, this.outOfFrameByteReceived);
                this.ValidateMessage(await S101Message.ReadFromAsync(this.deframedBuffer, cancellationToken));
            }

            return read;
        }

        private void ValidateMessage(S101Message newMessage)
        {
            if (newMessage == null)
            {
                throw new S101Exception("Unexpected end of stream.");
            }

            if (this.message.Slot != newMessage.Slot)
            {
                throw new S101Exception("Inconsistent Slot in multi-packet message.");
            }

            if (!this.message.Command.Equals(newMessage.Command))
            {
                throw new S101Exception("Inconsistent Command in multi-packet message.");
            }

            if ((newMessage.PacketFlags & PacketFlags.FirstPacket) > 0)
            {
                throw new S101Exception(string.Format(
                    CultureInfo.InvariantCulture, "{0} flag in subsequent packet.", PacketFlags.FirstPacket));
            }

            this.message = newMessage;
        }

        private Task<int> ReadDeframedAsync(byte[] buffer, int index, int count, CancellationToken cancellationToken) =>
            this.deframingStream.ReadAsync(buffer, index, count, cancellationToken);
    }
}
