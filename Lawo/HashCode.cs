////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo
{
    /// <summary>Provides helper methods for hash codes.</summary>
    public static class HashCode
    {
        /// <summary>Returns the combination of <paramref name="hash1"/> and <paramref name="hash2"/>.</summary>
        public static int Combine(int hash1, int hash2)
        {
            // Taken from the Tuple.CombineHashCodes
            return unchecked(((hash1 << 5) + 1) ^ hash2);
        }

        /// <summary>Returns the combination of <paramref name="hash1"/>, <paramref name="hash2"/> and
        /// <paramref name="hash3"/>.</summary>
        public static int Combine(int hash1, int hash2, int hash3)
        {
            return Combine(Combine(hash1, hash2), hash3);
        }
    }
}
