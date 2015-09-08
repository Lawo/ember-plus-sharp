////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>The exception that is thrown when the invocation of a function fails.</summary>
    /// <remarks>
    /// <para><b>Thread Safety</b>: Any public static members of this type are thread safe. Any instance members are not
    /// guaranteed to be thread safe.</para>
    /// </remarks>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification = "These ctors do not make much sense here.")]
    public sealed class InvocationFailedException : Exception
    {
        private readonly IResult result;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets the function result.</summary>
        public IResult Result
        {
            get { return this.result; }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal InvocationFailedException(string message, IResult result) : base(message)
        {
            this.result = result;
        }
    }
}
