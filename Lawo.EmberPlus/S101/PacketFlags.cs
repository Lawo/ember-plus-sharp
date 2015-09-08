////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.S101
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>Specifies the nature of a packet within a message.</summary>
    [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flags", Justification = "Flags is used in the specification.")]
    [SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32", Justification = "The protocol requires a byte.")]
    [Flags]
    internal enum PacketFlags : byte
    {
        /// <summary>The packet is an intermediate one in a multi-packet message (none of the other flags apply).
        /// </summary>
        None = 0x00,

        /// <summary>The packet is an empty one in a multi-packet message.</summary>
        EmptyPacket = 0x20,

        /// <summary>The packet is the last one in a multi-packet message.</summary>
        LastPacket = 0x40,

        /// <summary>The packet is the last one in a multi-packet message.</summary>
        FirstPacket = 0x80,

        /// <summary>The packet is the only one in the message.</summary>
        SinglePacket = LastPacket | FirstPacket
    }
}
