////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.Threading
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;

    /// <summary>Provides access to native methods.</summary>
    /// <threadsafety static="true" instance="false"/>
    public static class NativeMethods
    {
        /// <summary>Returns the return value of
        /// <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms683183.aspx">GetCurrentThreadId</see>.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Expensive method.")]
        [SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible", Justification = "Public access to this method is not a security concern.")]
        [DllImport("Kernel32.dll")]
        [CLSCompliant(false)]
        public static extern uint GetCurrentThreadId();
    }
}
