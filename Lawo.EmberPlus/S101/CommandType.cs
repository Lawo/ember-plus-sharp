////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.S101
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>Defines the identifiers used to indicate the type of command.</summary>
    [SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32", Justification = "The protocol requires a byte.")]
    internal enum CommandType : byte
    {
        /// <summary>Ember Data.</summary>
        EmberData,

        /// <summary>Keep Alive Request.</summary>
        KeepAliveRequest,

        /// <summary>Keep Alive Response.</summary>
        KeepAliveResponse
    }
}
