////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Ember
{
    /// <summary>See <a href="http://www.itu.int/ITU-T/studygroups/com17/languages/X.690-0207.pdf">BER</a>, chapter
    /// 8.1.2.2.</summary>
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
