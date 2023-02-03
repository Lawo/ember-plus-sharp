////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.Threading.Tasks
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>Provides helper methods for tasks.</summary>
    public static class TaskHelper
    {
        /// <summary>Completes either when <paramref name="task"/> completes or when
        /// <paramref name="timeoutMilliseconds"/> have elapsed, whichever comes first.</summary>
        /// <param name="task">The task to wait for.</param>
        /// <param name="timeoutMilliseconds">The maximum number of milliseconds to wait.</param>
        /// <returns><c>true</c> if the <paramref name="task"/> did not complete within
        /// <paramref name="timeoutMilliseconds"/>; otherwise, <c>false</c>,</returns>
        public static async Task<bool> TimeoutAsync(this Task task, int timeoutMilliseconds)
        {
            var source = new CancellationTokenSource();

            try
            {
                return (await Task.WhenAny(task, Task.Delay(timeoutMilliseconds, source.Token)).ConfigureAwait(false)) != task;
            }
            finally
            {
                source.Cancel();
                source.Dispose();
            }
        }
    }
}
