////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.Threading.Tasks
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    /// <summary>Represents a virtual queue for CPU-intensive work.</summary>
    /// <remarks>Enqueued work is guaranteed to be executed sequentially, in the order in which it was enqueued.
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "This is a queue and we want the name to express that fact.")]
    public sealed class WorkQueue
    {
        private Task previousWork = Task.FromResult(false);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Initializes a new instance of the <see cref="WorkQueue"/> class.</summary>
        public WorkQueue()
        {
        }

        /// <summary>Enqueues <paramref name="action"/>.</summary>
        /// <returns>A task that represents the execution of <paramref name="action"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="action"/> equals <c>null</c>.</exception>
        /// <remarks><paramref name="action"/> is executed on an arbitrary thread pool thread.</remarks>
        public Task Enqueue(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            return this.Enqueue(() => { action(); return false; });
        }

        /// <summary>Enqueues <paramref name="function"/>.</summary>
        /// <typeparam name="TResult">The type of the result returned by <paramref name="function"/>.</typeparam>
        /// <returns>A task that represents the execution of <paramref name="function"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="function"/> equals <c>null</c>.</exception>
        /// <remarks><paramref name="function"/> is executed on an arbitrary thread pool thread.</remarks>
        public Task<TResult> Enqueue<TResult>(Func<TResult> function)
        {
            if (function == null)
            {
                throw new ArgumentNullException("function");
            }

            var result = this.previousWork.IsCompleted ?
                Task.Run(function) : this.previousWork.ContinueWith(t => function(), TaskScheduler.Default);
            this.previousWork = result;
            return result;
        }
    }
}
