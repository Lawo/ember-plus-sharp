////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.S101
{
    internal static class Frame
    {
        internal const byte InvalidStart = 0xF8;
        internal const byte EscapeByte = 0xFD;
        internal const byte BeginOfFrame = 0xFE;
        internal const byte EndOfFrame = 0xFF;
        internal const byte EscapeXor = 0x20;
    }
}
