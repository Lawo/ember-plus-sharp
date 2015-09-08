////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model
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
