////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Ember
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>Provides constants for the number of an inner identifier of a EmBER-encoded data value.</summary>
    /// <threadsafety static="true" instance="false"/>
    public static class InnerNumber
    {
        /// <summary>The end of a sequence, a set or an application-defined data value.</summary>
        public const int EndContainer = 0;

        /// <summary>Boolean data value.</summary>
        public const int Boolean = BerBoolean.InnerNumber;

        /// <summary>Integer data value.</summary>
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", Justification = "Official EmBER name.")]
        public const int Integer = BerInteger.InnerNumber;

        /// <summary>Octetstring data value.</summary>
        public const int Octetstring = BerOctetstring.InnerNumber;

        /// <summary>Real data value.</summary>
        public const int Real = BerReal.InnerNumber;

        /// <summary>UTF8String data value.</summary>
        public const int Utf8String = BerUtf8String.InnerNumber;

        /// <summary>Relative object identifier data value.</summary>
        public const int RelativeObjectIdentifier = BerRelativeObjectIdentifier.InnerNumber;

        /// <summary>The start of a Sequence data value.</summary>
        public const int Sequence = BerSequence.InnerNumber;

        /// <summary>The start of a Set data value.</summary>
        public const int Set = BerSet.InnerNumber;

        /// <summary>The start of a first application-defined data value.</summary>
        /// <remarks>Client code usually defines its own constants for the type numbers of its application-defined
        /// types, which must all be greater than or equal to this constant.</remarks>
        public const int FirstApplication = 1 << 30;
    }
}
