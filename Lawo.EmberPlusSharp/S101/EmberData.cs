////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.S101
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using IO;

    using static System.Globalization.CultureInfo;
    using static System.Globalization.NumberStyles;

    /// <summary>Represents a command indicating that the following payload contains EmBER data.</summary>
    /// <remarks>See the <i>"Ember+ Specification"</i><cite>Ember+ Specification</cite>, chapter "Message Framing".
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    public sealed class EmberData : S101Command
    {
        /// <summary>Initializes a new instance of the <see cref="EmberData"/> class.</summary>
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", Justification = "Official EmBER name.")]
        public EmberData(byte dtd, params byte[] applicationBytes)
            : this()
        {
            this.dtd = dtd;
            this.applicationBytes = applicationBytes;
        }

        /// <summary>Gets the DTD.</summary>
        public byte Dtd => this.dtd;

        /// <summary>Gets the application bytes.</summary>
        public IReadOnlyCollection<byte> ApplicationBytes => this.applicationBytes;

        /// <inheritdoc/>
        public sealed override string ToString() =>
            base.ToString() + ' ' + this.dtd.ToString("X2", InvariantCulture) +
            (this.applicationBytes.Length > 0 ? " " : null) +
            string.Join(" ", this.applicationBytes.Select(b => b.ToString("X2", InvariantCulture)));

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal EmberData()
            : base(CommandType.EmberData)
        {
        }

        internal sealed override bool CanHavePayload => true;

        internal sealed override bool CanHaveMultiplePackets => true;

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
            writeBuffer[writeBuffer.Count++] = this.dtd;
            writeBuffer[writeBuffer.Count++] = (byte)this.applicationBytes.Length;
            await writeBuffer.WriteAsync(
                this.applicationBytes, 0, this.applicationBytes.Length, cancellationToken);
        }

        internal sealed override void ParseCore(string[] components)
        {
            base.ParseCore(components);
            this.dtd = byte.Parse(components[1], AllowHexSpecifier, InvariantCulture);
            this.applicationBytes =
                components.Skip(2).Select(s => byte.Parse(s, AllowHexSpecifier, InvariantCulture)).ToArray();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private byte dtd;
        private byte[] applicationBytes;

        private void GetPacketFlagsDtdAndAppBytesLength(ReadBuffer readBuffer)
        {
            this.PacketFlags = (PacketFlags)readBuffer[readBuffer.Index++];
            this.dtd = readBuffer[readBuffer.Index++];
            this.applicationBytes = new byte[readBuffer[readBuffer.Index++]];
        }
    }
}
