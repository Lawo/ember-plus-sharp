////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright 2008-2012 Andreas Huber Doenni, original from http://phuse.codeplex.com.
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo
{
    using System;
    using System.Globalization;

    /// <summary>Provides helper functions for buffers.</summary>
    /// <threadsafety static="true" instance="false"/>
    public static class BufferHelper
    {
        /// <summary>Checks that <paramref name="index"/> and <paramref name="count"/> form a valid range in
        /// <paramref name="buffer"/>.</summary>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> equals <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> and/or <paramref name="count"/>
        /// are negative.</exception>
        /// <exception cref="ArgumentException">The range defined by <paramref name="index"/> and
        /// <paramref name="count"/> does not fall entirely within the buffer.</exception>
        public static void AssertValidRange<T>(
            T[] buffer, string bufferName, int index, string indexName, int count, string countName)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(bufferName);
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(indexName, ExceptionMessages.NonnegativeNumberRequired);
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(countName, ExceptionMessages.NonnegativeNumberRequired);
            }

            if (buffer.Length - index < count)
            {
                const string Format = "{0} and {1} must define a range that falls entirely within the buffer.";
                throw new ArgumentException(
                    string.Format(CultureInfo.InvariantCulture, Format, indexName, countName), bufferName);
            }
        }
    }
}
