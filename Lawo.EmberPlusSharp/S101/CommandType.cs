////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.S101
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
        KeepAliveResponse,

        /// <summary>Provider Status.</summary>
        ProviderStatus
    }
}
