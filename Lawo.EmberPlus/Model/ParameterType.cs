////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>Indicates the value type of a parameter.</summary>
    [SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue", Justification = "Dictated by the protocol.")]
    public enum ParameterType
    {
        /// <summary>Integer type.</summary>
        Integer = 1,

        /// <summary>Real type.</summary>
        Real = 2,

        /// <summary>String type.</summary>
        String = 3,

        /// <summary>Boolean type.</summary>
        Boolean = 4,

        /// <summary>Trigger type.</summary>
        Trigger = 5,

        /// <summary>Enum type.</summary>
        Enum = 6,

        /// <summary>Octets type.</summary>
        Octets = 7
    }
}
