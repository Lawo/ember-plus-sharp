////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System;

    /// <summary>
    /// Indicates how a parameter can be accessed.
    /// Defines the possible values of the "access" field of GlowParameter (GlowAccess)
    /// </summary>
    [Flags]
    public enum ParameterAccess
    {
        /// <summary>No access</summary>
        None = 0,

        /// <summary>Read Only</summary>
        Read = 1,

        /// <summary>Write Only</summary>
        Write = 2,

        /// <summary>Read and Write</summary>
        ReadWrite = Read | Write
    }
}