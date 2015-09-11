////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo
{
    using System.Collections.Generic;

    /// <summary>Provides fast methods to compare values of generic types.</summary>
    /// <threadsafety static="true" instance="false"/>
    public static class GenericCompare
    {
        /// <summary>Determines whether the specified object instances are considered equal.</summary>
        /// <typeparam name="T">The type of the objects to compare.</typeparam>
        /// <remarks>Returns the return value of <see cref="EqualityComparer{T}.Equals"/> called on
        /// <see cref="EqualityComparer{T}.Default"/>.</remarks>
        public static bool Equals<T>(T obj1, T obj2)
        {
            return ComparerHolder<T>.Equals(obj1, obj2);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static class ComparerHolder<T>
        {
            private static readonly EqualityComparer<T> Comparer = EqualityComparer<T>.Default;

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////

            internal static bool Equals(T obj1, T obj2)
            {
                return Comparer.Equals(obj1, obj2);
            }
        }
    }
}
