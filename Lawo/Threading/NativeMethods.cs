////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.Threading
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>Provides access to native methods.</summary>
    /// <threadsafety static="true" instance="false"/>
    public static class NativeMethods
    {
        /// <summary>Returns an integer that represents a unique identifier for the current managed thread.</summary>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Expensive method.")]
        [CLSCompliant(false)]
        public static uint GetCurrentThreadId()
        {
            return Convert.ToUInt32(System.Threading.Thread.CurrentThread.ManagedThreadId);
        }
    }
}
