﻿////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright 2008-2012 Andreas Huber Doenni, original from http://phuse.codeplex.com.
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo
{
    using System;

    /// <summary>Helper class for creation of <see cref="ScopeGuard{T}"/> objects.</summary>
    /// <remarks>
    /// <para><b>Thread Safety</b>: Any public static members of this type are thread safe. Any instance members are not
    /// guaranteed to be thread safe.</para>
    /// </remarks>
    public static class ScopeGuard
    {
        /// <summary>Creates a new <see cref="ScopeGuard{T}"/> object.</summary>
        /// <typeparam name="T">The type of the resource to guard.</typeparam>
        /// <param name="resource">The resource object that should be disposed in the event of failure. Can be
        /// <c>null</c>.</param>
        /// <remarks>See <see cref="ScopeGuard{T}"/> for more information.</remarks>
        public static ScopeGuard<T> Create<T>(T resource) where T : IDisposable
        {
            return new ScopeGuard<T>(resource);
        }
    }
}