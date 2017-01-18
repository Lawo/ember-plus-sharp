////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Ember
{
    internal static class Constants
    {
        internal const int BitsPerByte = 8;
        internal const int BitsPerEncodedByte = 7; // Encoded bytes are used for identifiers and subidentifiers
        internal const int BytesPerLong = 8;
        internal const int BytesPerInt = 4;
        internal const int BitsPerLong = BytesPerLong * BitsPerByte;
        internal const int BitsPerInt = BytesPerInt * BitsPerByte;

        internal const long AllBitsSetLong = -1;
        internal const int AllBitsSetInt = -1;

        internal const long DoubleSignMask = long.MinValue;
        internal const int DoubleMantissaBits = 52;
        internal const long DoubleExponentMask = (AllBitsSetLong << DoubleMantissaBits) & ~DoubleSignMask;
        internal const long DoubleExponentBias = 1023;
        internal const long DoubleMantissaMask = ~(AllBitsSetLong << DoubleMantissaBits);

        /// <summary>The default size of the temporary in-memory buffer that is allocated to read from/write to a stream
        /// that is presumed to read/write its data from/to memory.</summary>
        internal const int MemoryStreamBufferSize = 1024;
    }
}