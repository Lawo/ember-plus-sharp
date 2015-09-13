////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.S101
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using IO;

    /// <summary>Represents a message excluding payload.</summary>
    /// <remarks>
    /// <para>See the <i>"Ember+ Specification"</i><cite>Ember+ Specification</cite>, chapter "Message Framing".</para>
    /// <para>Non-Ember messages are not currently supported.</para>
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    public sealed class S101Message
    {
        private readonly byte messageType;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Initializes a new instance of the <see cref="S101Message"/> class.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="command"/> equals <c>null</c>.</exception>
        public S101Message(byte slot, S101Command command) : this(slot, S101.MessageType.Ember, command)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
        }

        /// <summary>Gets the Slot.</summary>
        public byte Slot { get; private set; }

        /// <summary>Gets the Command.</summary>
        public S101Command Command { get; private set; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal bool CanHavePayload
        {
            get { return this.Command.CanHavePayload; }
        }

        internal bool CanHaveMultiplePackets
        {
            get { return this.Command.CanHaveMultiplePackets; }
        }

        internal PacketFlags PacketFlags
        {
            get { return this.Command.PacketFlags; }
            set { this.Command.PacketFlags = value; }
        }

        internal async Task WriteToAsync(WriteBuffer writerBuffer, CancellationToken cancellationToken)
        {
            await writerBuffer.ReserveAsync(2, cancellationToken);
            writerBuffer[writerBuffer.Count++] = this.Slot;
            writerBuffer[writerBuffer.Count++] = this.messageType;
            await this.Command.WriteToAsync(writerBuffer, cancellationToken);
        }

        internal static async Task<S101Message> ReadFromAsync(
            ReadBuffer readBuffer, CancellationToken cancellationToken)
        {
            if (!await readBuffer.ReadAsync(cancellationToken))
            {
                return null;
            }

            var slot = readBuffer[readBuffer.Index++];
            byte messageType;
            S101Command command;

            try
            {
                await readBuffer.FillAsync(1, cancellationToken);
                messageType = GetMessageType(readBuffer);
                command = await S101Command.ReadFromAsync(readBuffer, cancellationToken);
            }
            catch (EndOfStreamException ex)
            {
                throw new S101Exception("Unexpected end of stream.", ex);
            }

            return new S101Message(slot, messageType, command);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private S101Message(byte slot, byte messageType, S101Command command)
        {
            this.Slot = slot;
            this.messageType = messageType;
            this.Command = command;
        }

        private static byte GetMessageType(ReadBuffer readBuffer)
        {
            var messageType = readBuffer[readBuffer.Index++];

            if (messageType != S101.MessageType.Ember)
            {
                throw new S101Exception("Unexpected Message Type.");
            }

            return messageType;
        }
    }
}
