////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.S101
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>Provides information about a logged event.</summary>
    /// <threadsafety static="true" instance="false"/>
    [SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "There's no point in comparing instances of this type.")]
    public struct EventInfo
    {
        private readonly DateTime? timeUtc;
        private readonly int? number;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets the time when the event was logged.</summary>
        public DateTime? TimeUtc
        {
            get { return this.timeUtc; }
        }

        /// <summary>Gets the number of the event.</summary>
        public int? Number
        {
            get { return this.number; }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal EventInfo(DateTime? timeUtc) : this(timeUtc, null)
        {
        }

        internal EventInfo(DateTime? timeUtc, int? number)
        {
            this.timeUtc = timeUtc;
            this.number = number;
        }
    }
}
