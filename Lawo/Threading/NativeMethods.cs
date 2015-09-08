////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.Threading
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;

    /// <summary>Provides access to native methods.</summary>
    public static class NativeMethods
    {
        /// <summary>Returns the return value of
        /// <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms683183.aspx">GetCurrentThreadId</a>.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Expensive method.")]
        [SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible", Justification = "Public access to this method is not a security concern.")]
        [DllImport("Kernel32.dll")]
        [CLSCompliant(false)]
        public static extern uint GetCurrentThreadId();
    }
}
