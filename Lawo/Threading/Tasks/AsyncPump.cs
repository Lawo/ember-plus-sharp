////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Original from http://blogs.msdn.com/b/pfxteam/archive/2012/01/20/10259049.aspx.
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.Threading.Tasks
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>Represents a pump that runs an asynchronous method and all its continuations on the current thread.
    /// </summary>
    /// <remarks>Some asynchronous methods expect that all its continuations are executed on the same thread. If such
    /// code needs to be run in an environment where this is not guaranteed
    /// (<see cref="SynchronizationContext.Current"/> is either <c>null</c> or is a
    /// <see cref="SynchronizationContext"/> object that schedules continuations on different threads as under ASP.NET)
    /// then this class can be used to force execution on a single thread.</remarks>
    /// <threadsafety static="true" instance="false"/>
    public static class AsyncPump
    {
        /// <summary>Runs <paramref name="asyncMethod"/> on the current thread.</summary>
        /// <exception cref="Exception"><paramref name="asyncMethod"/> completed in the <see cref="TaskStatus.Faulted"/>
        /// state.</exception>
        /// <summary>Add <paramref name="cancellationToken"/> for operation control.</summary>
        public static void Run(Func<Task> asyncMethod, CancellationToken cancellationToken)
        {
            if (asyncMethod == null)
            {
                throw new ArgumentNullException(nameof(asyncMethod));
            }

            if (cancellationToken == null)
            {
                throw new ArgumentNullException(nameof(cancellationToken));
            }

            var previousContext = SynchronizationContext.Current;
            var newContext = new SingleThreadSynchronizationContext();

            try
            {
                SynchronizationContext.SetSynchronizationContext(newContext);
                newContext.OperationStarted();
                var task = asyncMethod();

                if (task == null)
                {
                    newContext.OperationCompleted();
                    throw new ArgumentException("The method returned null.", nameof(asyncMethod));
                }

                task.ContinueWith(t => newContext.OperationCompleted(), cancellationToken, TaskContinuationOptions.NotOnCanceled, TaskScheduler.Default);
                newContext.RunOnCurrentThread();
                task.GetAwaiter().GetResult();
            }
            finally
            {
                newContext.Dispose();
                SynchronizationContext.SetSynchronizationContext(previousContext);
            }
        }

        private sealed class SingleThreadSynchronizationContext : SynchronizationContext, IDisposable
        {
            public void Dispose() => this.queue.Dispose();

            public sealed override SynchronizationContext CreateCopy() => this;

            public sealed override void OperationStarted() => Interlocked.Increment(ref this.operationCount);

            public sealed override void OperationCompleted()
            {
                if (Interlocked.Decrement(ref this.operationCount) == 0)
                {
                    this.queue.CompleteAdding();
                }
            }

            public sealed override void Post(SendOrPostCallback d, object state) => this.queue.Add(new KeyValuePair<SendOrPostCallback, object>(d ?? throw new ArgumentNullException(nameof(d)), state ?? ""));

            public sealed override void Send(SendOrPostCallback d, object state) =>
                throw new NotSupportedException("Send is not supported.");

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////

            internal void RunOnCurrentThread()
            {
                foreach (var workItem in this.queue.GetConsumingEnumerable())
                {
                    workItem.Key(workItem.Value);
                }
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////

            private readonly BlockingCollection<KeyValuePair<SendOrPostCallback, object>> queue =
                new BlockingCollection<KeyValuePair<SendOrPostCallback, object>>();

            private int operationCount;
        }
    }
}