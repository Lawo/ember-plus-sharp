////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.S101
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Lawo.IO;

    /// <summary>Represents a command that classifies the Ember data in the payload that follows.</summary>
    /// <remarks>
    /// <para>See <a href="http://ember-plus.googlecode.com/svn/trunk/documentation/Ember+%20Documentation.pdf">Ember+
    /// Specification</a>, Chapter "Message Framing".</para>
    /// <para><b>Thread Safety</b>: Any public static members of this type are thread safe. Any instance members are not
    /// guaranteed to be thread safe.</para>
    /// </remarks>
    public sealed class EmberData : S101Command
    {
        private byte dtd;
        private byte[] applicationBytes;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Initializes a new instance of the <see cref="EmberData"/> class.</summary>
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", Justification = "Official EmBER name.")]
        public EmberData(byte dtd, params byte[] applicationBytes) : this()
        {
            this.dtd = dtd;
            this.applicationBytes = applicationBytes;
        }

        /// <summary>Gets the DTD.</summary>
        public byte Dtd
        {
            get { return this.dtd; }
        }

        /// <summary>Gets the application bytes.</summary>
        public IReadOnlyCollection<byte> ApplicationBytes
        {
            get { return this.applicationBytes; }
        }

        /// <summary>See <see cref="object.ToString"/>.</summary>
        public sealed override string ToString()
        {
            return base.ToString() + ' ' + dtd.ToString("X2", CultureInfo.InvariantCulture) +
                (this.applicationBytes.Length > 0 ? " " : null) +
                string.Join(" ", this.applicationBytes.Select(b => b.ToString("X2", CultureInfo.InvariantCulture)));
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal EmberData() : base(CommandType.EmberData)
        {
        }

        internal sealed override bool CanHavePayload
        {
            get { return true; }
        }

        internal sealed override bool CanHaveMultiplePackets
        {
            get { return true; }
        }

        internal sealed override async Task ReadFromCoreAsync(ReadBuffer readBuffer, CancellationToken cancellationToken)
        {
            await base.ReadFromCoreAsync(readBuffer, cancellationToken);
            await readBuffer.FillAsync(3, cancellationToken);
            this.GetPacketFlagsDtdAndAppBytesLength(readBuffer);
            await readBuffer.FillAsync(
                this.applicationBytes, 0, this.applicationBytes.Length, cancellationToken);
        }

        internal sealed override async Task WriteToCoreAsync(
            WriteBuffer writeBuffer, CancellationToken cancellationToken)
        {
            await base.WriteToCoreAsync(writeBuffer, cancellationToken);
            await writeBuffer.ReserveAsync(3, cancellationToken);
            writeBuffer[writeBuffer.Count++] = (byte)this.PacketFlags;
            writeBuffer[writeBuffer.Count++] = (byte)this.dtd;
            writeBuffer[writeBuffer.Count++] = (byte)this.applicationBytes.Length;
            await writeBuffer.WriteAsync(
                this.applicationBytes, 0, this.applicationBytes.Length, cancellationToken);
        }

        internal sealed override void ParseCore(string[] components)
        {
            base.ParseCore(components);
            this.dtd = byte.Parse(components[1], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
            this.applicationBytes = components.Skip(2).Select(
                s => byte.Parse(s, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture)).ToArray();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void GetPacketFlagsDtdAndAppBytesLength(ReadBuffer readBuffer)
        {
            this.PacketFlags = (PacketFlags)readBuffer[readBuffer.Index++];
            this.dtd = readBuffer[readBuffer.Index++];
            this.applicationBytes = new byte[readBuffer[readBuffer.Index++]];
        }
    }
}
