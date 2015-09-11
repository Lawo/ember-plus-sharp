////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Ember
{
    /// <summary>See <b>X.690</b><cite>X.690</cite>, chapter 8.1.2.2.</summary>
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
