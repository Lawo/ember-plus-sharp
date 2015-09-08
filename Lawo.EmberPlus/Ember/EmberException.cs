////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Ember
{
    using System;

    /// <summary>The exception that is thrown when an error occurs while parsing EmBER-encoded data.</summary>
    /// <remarks>
    /// <para><b>Thread Safety</b>: Any public static members of this type are thread safe. Any instance members are not
    /// guaranteed to be thread safe.</para>
    /// </remarks>
    public sealed class EmberException : Exception
    {
        /// <summary>Initializes a new instance of the <see cref="EmberException"/> class.</summary>
        public EmberException() : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="EmberException"/> class.</summary>
        public EmberException(string message) : this(message, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="EmberException"/> class.</summary>
        public EmberException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
