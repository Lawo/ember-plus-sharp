////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System;

    /// <summary>The exception that is thrown when a model-related error occurs.</summary>
    /// <threadsafety static="true" instance="false"/>
    public sealed class ModelException : Exception
    {
        /// <summary>Initializes a new instance of the <see cref="ModelException"/> class.</summary>
        public ModelException()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ModelException"/> class.</summary>
        public ModelException(string message)
            : this(message, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ModelException"/> class.</summary>
        public ModelException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
