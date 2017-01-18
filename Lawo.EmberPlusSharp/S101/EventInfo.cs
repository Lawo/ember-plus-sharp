////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.S101
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>Provides information about a logged event.</summary>
    /// <threadsafety static="true" instance="false"/>
    [SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "There's no point in comparing instances of this type.")]
    public struct EventInfo
    {
        /// <summary>Gets the time when the event was logged.</summary>
        public DateTime? TimeUtc { get; }

        /// <summary>Gets the number of the event.</summary>
        public int? Number { get; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal EventInfo(DateTime? timeUtc)
            : this(timeUtc, null)
        {
        }

        internal EventInfo(DateTime? timeUtc, int? number)
        {
            this.TimeUtc = timeUtc;
            this.Number = number;
        }
    }
}
