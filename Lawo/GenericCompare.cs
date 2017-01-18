////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
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
        /// <remarks>Returns the return value of <see cref="EqualityComparer{T}.Equals(T, T)"/> called on
        /// <see cref="EqualityComparer{T}.Default"/>.</remarks>
        public static bool Equals<T>(T obj1, T obj2) => ComparerHolder<T>.Equals(obj1, obj2);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static class ComparerHolder<T>
        {
            internal static bool Equals(T obj1, T obj2) => Comparer.Equals(obj1, obj2);

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////

            private static readonly EqualityComparer<T> Comparer = EqualityComparer<T>.Default;
        }
    }
}
