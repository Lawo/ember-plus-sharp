////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.S101
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
