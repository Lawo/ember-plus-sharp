////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    internal enum RetrieveDetailsChangeStatus
    {
        /// <summary><see cref="Element.RetrieveDetails"/> has the value set during initialization or first read
        /// operation.</summary>
        Initialized,

        /// <summary><see cref="Element.RetrieveDetails"/> has not been changed since
        /// <see cref="Element.RetrieveDetailsChangeStatus"/> has been reset.</summary>
        Unchanged,

        /// <summary><see cref="Element.RetrieveDetails"/> has been changed since
        /// <see cref="Element.RetrieveDetailsChangeStatus"/> has been reset.</summary>
        Changed
    }
}
