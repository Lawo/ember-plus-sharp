////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright 2008-2012 Andreas Huber Doenni, original from http://phuse.codeplex.com.
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo
{
    using System;

    /// <summary>Helper class for creation of <see cref="ScopeGuard{T}"/> objects.</summary>
    /// <threadsafety static="true" instance="false"/>
    public static class ScopeGuard
    {
        /// <summary>Creates a new <see cref="ScopeGuard{T}"/> object.</summary>
        /// <typeparam name="T">The type of the resource to guard.</typeparam>
        /// <param name="resource">The resource object that should be disposed in the event of failure. Can be
        /// <c>null</c>.</param>
        /// <seealso cref="ScopeGuard{T}"/>
        public static ScopeGuard<T> Create<T>(T resource)
            where T : IDisposable
        {
            return new ScopeGuard<T>(resource);
        }
    }
}
