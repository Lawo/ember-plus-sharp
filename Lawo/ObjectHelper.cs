////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>Provides helper methods for objects.</summary>
    public static class ObjectHelper
    {
        /// <summary>Ignores <paramref name="obj"/>.</summary>
        /// <typeparam name="T">The type of the object to ignore.</typeparam>
        /// <remarks>This method is primarily useful to silence compiler and CA warnings.</remarks>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "The whole purpose of this function is to ignore the argument.")]
        public static void Ignore<T>(this T obj)
        {
        }
    }
}
