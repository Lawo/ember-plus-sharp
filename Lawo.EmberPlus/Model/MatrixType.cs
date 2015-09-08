////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model
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