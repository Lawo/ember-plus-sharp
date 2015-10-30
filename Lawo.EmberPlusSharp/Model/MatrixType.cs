////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    /// <summary>Indicates the type of a matrix.</summary>
    public enum MatrixType
    {
        /// <summary>1:N matrix.</summary>
        OneToN = 0,

        /// <summary>1:1 matrix.</summary>
        OneToOne = 1,

        /// <summary>N:N matrix.</summary>
        NToN = 2
    }
}