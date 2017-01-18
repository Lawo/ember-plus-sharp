////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>Represents a function accepting two arguments in the object tree accessible through
    /// <see cref="Consumer{T}.Root">Consumer&lt;TRoot&gt;.Root</see>.</summary>
    /// <typeparam name="T1">The type of the first argument.</typeparam>
    /// <typeparam name="T2">The type of the second argument.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <threadsafety static="true" instance="false"/>
    [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Function", Justification = "Intentional, usability from other languages is not a priority.")]
    public sealed class Function<T1, T2, TResult> : StaticFunction<Function<T1, T2, TResult>>
        where TResult : ResultBase<TResult>, new()
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Schedules an invocation of this function.</summary>
        /// <exception cref="InvocationFailedException">The provider reported that the invocation failed.</exception>
        /// <remarks>The invocation is sent automatically within the interval defined by
        /// <see cref="Consumer{T}.AutoSendInterval"/>. When
        /// <see cref="Consumer{T}.AutoSendInterval"/> equals <see cref="Timeout.Infinite"/>,
        /// <see cref="Consumer{T}.SendAsync"/> must be called before awaiting the returned task.</remarks>
        public Task<TResult> InvokeAsync(T1 arg1, T2 arg2) =>
            this.InvokeCoreAsync(
                new TResult(), new ValueWriter<T1>(arg1).WriteValue, new ValueWriter<T2>(arg2).WriteValue);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private Function()
        {
        }
    }
}
