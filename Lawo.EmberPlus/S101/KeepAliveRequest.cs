////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.S101
{
    /// <summary>Represents a Keep Alive Request.</summary>
    /// <remarks>See the <b>Ember+ Specification</b><cite>Ember+ Specification</cite>, chapter "Message Framing".
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    public sealed class KeepAliveRequest : S101Command
    {
        /// <summary>Initializes a new instance of the <see cref="KeepAliveRequest"/> class.</summary>
        public KeepAliveRequest() : base(CommandType.KeepAliveRequest)
        {
        }
    }
}
