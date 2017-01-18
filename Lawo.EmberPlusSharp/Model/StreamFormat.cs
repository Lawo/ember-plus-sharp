////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    /// <summary>Indicates the format of a value in a stream.</summary>
    public enum StreamFormat
    {
        /// <summary><see cref="byte"/>.</summary>
        Byte = 0,

        /// <summary>Big endian <see cref="ushort"/>.</summary>
        UInt16BigEndian = 2,

        /// <summary>Little endian <see cref="ushort"/>.</summary>
        UInt16LittleEndian = 3,

        /// <summary>Big endian <see cref="uint"/>.</summary>
        UInt32BigEndian = 4,

        /// <summary>Little endian <see cref="uint"/>.</summary>
        UInt32LittleEndian = 5,

        /// <summary>Big endian <see cref="ulong"/>.</summary>
        UInt64BigEndian = 6,

        /// <summary>Little endian <see cref="ulong"/>.</summary>
        UInt64LittleEndian = 7,

        /// <summary><see cref="sbyte"/>.</summary>
        SByte = 8,

        /// <summary>Big endian <see cref="short"/>.</summary>
        Int16BigEndian = 10,

        /// <summary>Little endian <see cref="short"/>.</summary>
        Int16LittleEndian = 11,

        /// <summary>Big endian <see cref="int"/>.</summary>
        Int32BigEndian = 12,

        /// <summary>Little endian <see cref="int"/>.</summary>
        Int32LittleEndian = 13,

        /// <summary>Big endian <see cref="long"/>.</summary>
        Int64BigEndian = 14,

        /// <summary>Little endian <see cref="long"/>.</summary>
        Int64LittleEndian = 15,

        /// <summary>Big endian <see cref="float"/>.</summary>
        Float32BigEndian = 20,

        /// <summary>Little endian <see cref="float"/>.</summary>
        Float32LittleEndian = 21,

        /// <summary>Big endian <see cref="double"/>.</summary>
        Float64BigEndian = 22,

        /// <summary>Little endian <see cref="double"/>.</summary>
        Float64LittleEndian = 23
    }
}