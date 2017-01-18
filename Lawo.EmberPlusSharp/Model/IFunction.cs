////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>Provides the common interface for all functions in the object tree accessible through
    /// <see cref="Consumer{T}.Root">Consumer&lt;TRoot&gt;.Root</see>.</summary>
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
        Task<IResult> InvokeAsync(params object[] actualArguments);
    }
}
