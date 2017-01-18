////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Ember
{
    using System;

    /// <summary>The exception that is thrown when an error occurs while parsing EmBER-encoded data.</summary>
    /// <threadsafety static="true" instance="false"/>
    public sealed class EmberException : Exception
    {
        /// <summary>Initializes a new instance of the <see cref="EmberException"/> class.</summary>
        public EmberException()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="EmberException"/> class.</summary>
        public EmberException(string message)
            : this(message, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="EmberException"/> class.</summary>
        public EmberException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
