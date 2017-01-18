////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.S101
{
    /// <summary>Represents a Keep Alive Response.</summary>
    /// <remarks>See the <i>"Ember+ Specification"</i><cite>Ember+ Specification</cite>, chapter "Message Framing".
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    public sealed class KeepAliveResponse : S101Command
    {
        /// <summary>Initializes a new instance of the <see cref="KeepAliveResponse"/> class.</summary>
        public KeepAliveResponse()
            : base(CommandType.KeepAliveResponse)
        {
        }
    }
}
