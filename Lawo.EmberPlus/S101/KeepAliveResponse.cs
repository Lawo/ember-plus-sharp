////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.S101
{
    /// <summary>Represents a Keep Alive Response.</summary>
    /// <remarks>See the <i>"Ember+ Specification"</i><cite>Ember+ Specification</cite>, chapter "Message Framing".
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    public sealed class KeepAliveResponse : S101Command
    {
        /// <summary>Initializes a new instance of the <see cref="KeepAliveResponse"/> class.</summary>
        public KeepAliveResponse() : base(CommandType.KeepAliveResponse)
        {
        }
    }
}
