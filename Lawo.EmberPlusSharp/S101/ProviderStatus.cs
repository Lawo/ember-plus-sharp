////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.S101
{
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;

    using IO;

    /// <summary>Represents a provider status.</summary>
    /// <remarks>See the <i>"Ember+ Specification"</i><cite>Ember+ Specification</cite>, chapter "Message Framing".
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    public sealed class ProviderStatus : S101Command
    {
        /// <summary>Initializes a new instance of the <see cref="ProviderStatus"/> class.</summary>
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", Justification = "Official EmBER name.")]
        public ProviderStatus(bool isActive)
            : this()
        {
            this.IsActive = isActive;
        }

        /// <summary>Gets a value indicating whether the provider is active or passive.</summary>
        public bool IsActive { get; private set; }

        /// <inheritdoc/>
        public sealed override string ToString() => base.ToString() + " 0" + (this.IsActive ? '1' : '0');

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal ProviderStatus()
            : base(CommandType.ProviderStatus)
        {
        }

        internal sealed override bool CanHavePayload => false;

        internal sealed override bool CanHaveMultiplePackets => false;

        internal sealed override async Task ReadFromCoreAsync(
            ReadBuffer readBuffer, CancellationToken cancellationToken)
        {
            await base.ReadFromCoreAsync(readBuffer, cancellationToken);
            await readBuffer.FillAsync(1, cancellationToken);
            this.IsActive = readBuffer[readBuffer.Index++] > 0;
        }

        internal sealed override async Task WriteToCoreAsync(
            WriteBuffer writeBuffer, CancellationToken cancellationToken)
        {
            await base.WriteToCoreAsync(writeBuffer, cancellationToken);
            await writeBuffer.ReserveAsync(1, cancellationToken);
            writeBuffer[writeBuffer.Count++] = (byte)(this.IsActive ? 1 : 0);
        }

        internal sealed override void ParseCore(string[] components)
        {
            base.ParseCore(components);
            this.IsActive = byte.Parse(components[1], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture) > 0;
        }
    }
}
