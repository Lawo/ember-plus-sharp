////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.S101
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    using IO;

    /// <summary>Transparently encodes a single frame.</summary>
    /// <remarks>
    /// <para>At construction, the BOF is first appended to the <see cref="WriteBuffer"/> object passed to
    /// <see cref="FramingStream.CreateAsync"/>. Afterwards, when data is written to this stream
    /// then it is first encoded and the encoded form is then appended to the <see cref="WriteBuffer"/> object.</para>
    /// <para><b>Caution</b>: <see cref="DisposeAsync"/> <b>must</b> be called in the end.</para>
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    internal sealed class FramingStream : BufferStream
    {
        public sealed override async Task DisposeAsync(CancellationToken cancellationToken)
        {
            if (!this.IsDisposed)
            {
                var invertedCrc = (ushort)(~this.crc & ushort.MaxValue);
                var crcBytes =
                    new[] { (byte)(invertedCrc & byte.MaxValue), (byte)((invertedCrc >> 8) & byte.MaxValue) };
                await this.WriteAsync(crcBytes, 0, crcBytes.Length, cancellationToken);

                var writeBuffer = this.WriteBuffer;
                await writeBuffer.ReserveAsync(1, cancellationToken);
                writeBuffer[writeBuffer.Count++] = Frame.EndOfFrame;
                await base.DisposeAsync(cancellationToken);
            }
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated with call to BufferHelper.AssertValidRange.")]
        public sealed override async Task WriteAsync(
            byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var writeBuffer = this.WriteBuffer;
            var pastEnd = offset + count;

            while ((offset < pastEnd) && ((writeBuffer.Count < writeBuffer.Capacity) ||
                await writeBuffer.FlushAsync(cancellationToken)))
            {
                offset = this.WriteByte(buffer, offset, writeBuffer);
            }

            this.TotalCount += count;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal static async Task<FramingStream> CreateAsync(
            WriteBuffer writeBuffer, CancellationToken cancellationToken)
        {
            await writeBuffer.ReserveAsync(1, cancellationToken);
            return new FramingStream(writeBuffer);
        }

        internal int TotalCount { get; private set; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private ushort crc = 0xFFFF;
        private bool previousWasEscapeByte;

        private FramingStream(WriteBuffer writeBuffer)
            : base(null, writeBuffer)
        {
            writeBuffer[writeBuffer.Count++] = Frame.BeginOfFrame;
        }

        private int WriteByte(byte[] buffer, int offset, WriteBuffer writeBuffer)
        {
            // The body of this method should rather be inlined where it is called, but doing so seems to cause a huge
            // (>5x) perf hit.
            var currentByte = buffer[offset];

            if (this.previousWasEscapeByte)
            {
                this.previousWasEscapeByte = false;
                this.crc = Crc.AddCrcCcitt(this.crc, currentByte);
                currentByte = (byte)(currentByte ^ Frame.EscapeXor);
                ++offset;
            }
            else
            {
                if (currentByte < Frame.InvalidStart)
                {
                    this.crc = Crc.AddCrcCcitt(this.crc, currentByte);
                    ++offset;
                }
                else
                {
                    currentByte = Frame.EscapeByte;
                    this.previousWasEscapeByte = true;
                }
            }

            writeBuffer[writeBuffer.Count++] = currentByte;
            return offset;
        }
    }
}
