////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Glow
{
    /// <summary>
    /// Defines the possible values of the "number" field of GlowCommand.
    /// </summary>
    internal enum GlowCommandNumber : long
    {
        /// <summary><c>Subscribe</c> command</summary>
        Subscribe = 30,

        /// <summary><c>Unsubscribe</c> command</summary>
        Unsubscribe = 31,

        /// <summary><c>GetDirectory</c> command</summary>
        GetDirectory = 32,

        /// <summary><c>Invoke</c> command</summary>
        Invoke = 33
    }
}
