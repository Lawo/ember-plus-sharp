////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2016 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    internal enum IsOnlineChangeStatus
    {
        /// <summary><see cref="Element.IsOnline"/> has the value set during initialization or first read operation.
        /// </summary>
        Initialized,

        /// <summary><see cref="Element.IsOnline"/> has not been changed since
        /// <see cref="Element.IsOnlineChangeStatus"/> has been reset.</summary>
        Unchanged,

        /// <summary><see cref="Element.IsOnline"/> has been changed since
        /// <see cref="Element.IsOnlineChangeStatus"/> has been reset.</summary>
        Changed
    }
}
