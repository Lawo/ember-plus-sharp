////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>Represents a function in the protocol specified in the
    /// <a href="http://ember-plus.googlecode.com/svn/trunk/documentation/Ember+%20Documentation.pdf">Ember+
    /// Specification</a>.</summary>
    public interface IFunction : IElement
    {
        /// <summary>Gets <b>arguments</b>.</summary>
        IReadOnlyList<KeyValuePair<string, ParameterType>> Arguments { get; }

        /// <summary>Gets <b>result</b>.</summary>
        IReadOnlyList<KeyValuePair<string, ParameterType>> Result { get; }

        /// <summary>Schedules an invocation of this function.</summary>
        /// <exception cref="ArgumentException"><list type="bullet">
        /// <item>The <see cref="Array.Length"/> property of <paramref name="actualArguments"/> is not equal to the
        /// <see cref="IReadOnlyCollection{T}.Count"/> property of <see cref="Arguments"/>, or</item>
        /// <item>the type of at least one element in <paramref name="actualArguments"/> does not match the type of the
        /// corresponding element in <see cref="Arguments"/>.</item>
        /// </list></exception>
        /// <exception cref="InvocationFailedException">The provider reported that the invocation failed.</exception>
        /// <remarks>The invocation is sent automatically within the interval defined by
        /// <see cref="Consumer{T}.AutoSendInterval"/>. When
        /// <see cref="Consumer{T}.AutoSendInterval"/> equals <see cref="Timeout.Infinite"/>,
        /// <see cref="Consumer{T}.SendAsync"/> must be called before awaiting the returned task.</remarks>
        Task<IResult> Invoke(params object[] actualArguments);
    }
}
