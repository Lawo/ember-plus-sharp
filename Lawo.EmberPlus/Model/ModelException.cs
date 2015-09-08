////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model
{
    using System;

    /// <summary>The exception that is thrown when a model-related error occurs.</summary>
    /// <remarks>
    /// <para><b>Thread Safety</b>: Any public static members of this type are thread safe. Any instance members are not
    /// guaranteed to be thread safe.</para>
    /// </remarks>
    public sealed class ModelException : Exception
    {
        /// <summary>Initializes a new instance of the <see cref="ModelException"/> class.</summary>
        public ModelException() : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ModelException"/> class.</summary>
        public ModelException(string message) : this(message, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ModelException"/> class.</summary>
        public ModelException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
