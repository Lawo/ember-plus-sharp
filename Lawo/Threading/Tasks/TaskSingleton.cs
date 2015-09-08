////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.Threading.Tasks
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    /// <summary>Provides the means to simplify the implementation of non-reentrant async methods.</summary>
    /// <remarks>
    /// <para><b>Thread Safety</b>: Any public static members of this type are thread safe. Any instance members are not
    /// guaranteed to be thread safe.</para>
    /// </remarks>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "This is a queue and we want the name to express that fact.")]
    public sealed class TaskSingleton
    {
        private Task previousTask = Task.FromResult(false);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Executes <paramref name="function"/>.</summary>
        /// <returns>A <see cref="Task"/> object that represents a proxy for the <see cref="Task"/> returned by
        /// <paramref name="function"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="function"/> equals <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">The <see cref="Task.IsCompleted"/> property is <c>false</c> for
        /// the <see cref="Task"/> object returned by a previously called async method.</exception>
        /// <remarks><paramref name="function"/> is executed on the calling thread.</remarks>
        public Task Execute(Func<Task> function)
        {
            if (function == null)
            {
                throw new ArgumentNullException("function");
            }

            return this.Execute(async () => { await function(); return false; });
        }

        /// <summary>Executes <paramref name="function"/>.</summary>
        /// <typeparam name="TResult">The type of the result returned by the <see cref="Task{T}"/> returned by
        /// <paramref name="function"/>.</typeparam>
        /// <returns>The <see cref="Task{T}"/> object returned by <paramref name="function"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="function"/> equals <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">The <see cref="Task.IsCompleted"/> property is <c>false</c> for
        /// the <see cref="Task"/> object returned by a previously called async method.</exception>
        /// <remarks><paramref name="function"/> is executed on the calling thread.</remarks>
        public Task<TResult> Execute<TResult>(Func<Task<TResult>> function)
        {
            if (function == null)
            {
                throw new ArgumentNullException("function");
            }

            this.AssertTaskIsCompleted();
            var result = function();
            this.previousTask = result;
            return result;
        }

        /// <summary>Asserts that the previously executed task is completed.</summary>
        /// <exception cref="InvalidOperationException">The <see cref="Task.IsCompleted"/> property is <c>false</c> for
        /// the <see cref="Task"/> object returned by a previously called async method.</exception>
        public void AssertTaskIsCompleted()
        {
            if (!this.previousTask.IsCompleted)
            {
                throw new InvalidOperationException("The object is currently in use by a previous async operation.");
            }
        }
    }
}
