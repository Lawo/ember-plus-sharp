////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo
{
    /// <summary>Provides helper methods for hash codes.</summary>
    /// <threadsafety static="true" instance="false"/>
    public static class HashCode
    {
        /// <summary>Returns the combination of <paramref name="hash1"/> and <paramref name="hash2"/>.</summary>
        public static int Combine(int hash1, int hash2) => unchecked(((hash1 << 5) + 1) ^ hash2);

        /// <summary>Returns the combination of <paramref name="hash1"/>, <paramref name="hash2"/> and
        /// <paramref name="hash3"/>.</summary>
        public static int Combine(int hash1, int hash2, int hash3) => Combine(Combine(hash1, hash2), hash3);
    }
}
