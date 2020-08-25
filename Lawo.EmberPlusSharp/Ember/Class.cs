////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Ember
{
    /// <summary>
    /// BER tag classes
    /// See <i>"X.690"</i><cite>X.690</cite>, chapter 8.1.2.2.
    /// </summary>
    internal enum Class
    {
        /// <summary>Universal class.</summary>
        Universal = 0x00,

        /// <summary>Application class.</summary>
        Application = 0x40,

        /// <summary>Context-specific class.</summary>
        ContextSpecific = 0x80,

        /// <summary>Private class.</summary>
        Private = 0xC0
    }
}
