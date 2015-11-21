////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.GlowAnalyzerProxy.Main
{
    using System;

    internal sealed class ScrollEventIntoViewEventArgs : EventArgs
    {
        private readonly Event evt;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal ScrollEventIntoViewEventArgs(Event evt)
        {
            this.evt = evt;
        }

        internal Event NewEvent
        {
            get { return this.evt; }
        }
    }
}
