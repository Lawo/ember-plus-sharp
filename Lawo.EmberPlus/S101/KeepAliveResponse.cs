////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.S101
{
    /// <summary>Represents a Keep Alive Response.</summary>
    /// <remarks>
    /// <para>See <a href="http://ember-plus.googlecode.com/svn/trunk/documentation/Ember+%20Documentation.pdf">Ember+
    /// Specification</a>, Chapter "Message Framing".</para>
    /// <para><b>Thread Safety</b>: Any public static members of this type are thread safe. Any instance members are not
    /// guaranteed to be thread safe.</para>
    /// </remarks>
    public sealed class KeepAliveResponse : S101Command
    {
        /// <summary>Initializes a new instance of the <see cref="KeepAliveResponse"/> class.</summary>
        public KeepAliveResponse() : base(CommandType.KeepAliveResponse)
        {
        }
    }
}
