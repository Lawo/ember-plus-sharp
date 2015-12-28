////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.GlowAnalyzerProxy.Main
{
    using System;
    using System.Net.Sockets;

    internal sealed class ListenFailedEventArgs : EventArgs
    {
        private readonly SocketException exception;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal ListenFailedEventArgs(SocketException exception)
        {
            this.exception = exception;
        }

        internal SocketException Exception
        {
            get { return this.exception; }
        }
    }
}
