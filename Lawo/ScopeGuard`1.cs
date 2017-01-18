////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright 2008-2012 Andreas Huber Doenni, original from http://phuse.codeplex.com.
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>Simplifies resource object handling in functions that only create resource objects (but do not dispose
    /// them).</summary>
    /// <typeparam name="T">The type of the resource object.</typeparam>
    /// <remarks>
    /// <para>Often, a function needs to do other stuff besides constructing the resource object (e.g. creating child
    /// resource objects that the main resource will own or setting some properties after creation). In .NET pretty much
    /// any of these operations can fail by throwing an exception. Without the help of this class, one try-catch block
    /// would be necessary for the creation of each of these resource objects. In the try block the resource is created
    /// and possibly initialized, in the catch block the resource is disposed and the exception rethrown.</para>
    /// <para>This class is an adapted version of the facility presented in
    /// <see href="http://www.ddj.com/dept/cpp/184403758">this article</see>.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// StreamWriter CreateWriter()
    /// {
    ///     using (ScopeGuard&lt;FileStream&gt; streamGuard =
    ///         ScopeGuard.Create(new FileStream("C:\test.txt", FileMode.CreateNew)))
    ///     using (ScopeGuard&lt;StreamWriter&gt; writerGuard =
    ///         // At this point, the FileStream object is automatically disposed if the StreamWriter constructor throws
    ///         ScopeGuard.Create(new StreamWriter(streamGuard.Resource)))
    ///     {
    ///         // Here both resource objects are automatically disposed if the following statement throws
    ///         writerGuard.Resource.AutoFlush = true;
    ///         writerGuard.Dismiss(); // The whole creation succeeded -> dismiss both guards
    ///         streamGuard.Dismiss();
    ///         return writerGuard.Resource;
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <threadsafety static="true" instance="false"/>
    [SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "These operations do not make sense on this type.")]
    public sealed class ScopeGuard<T> : IDisposable
        where T : IDisposable
    {
        /// <summary>Gets the resource object passed to <see cref="ScopeGuard.Create{T}"/>.</summary>
        /// <exception cref="ObjectDisposedException">The scope guard has already been disposed.</exception>
        public T Resource
        {
            get
            {
                this.AssertNotDisposed();
                return this.resource;
            }
        }

        /// <summary>Calls <see cref="IDisposable.Dispose"/> on the resource object passed to the constructor <b>if and
        /// only if </b><see cref="Dismiss"/> and <see cref="Dispose"/> have never been called before.</summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Dispose() must never throw.")]
        public void Dispose()
        {
            if (this.disposed || this.dismissed)
            {
                return;
            }

            try
            {
                if (this.resource != null)
                {
                    this.resource.Dispose();
                }
            }
            catch
            {
            }
            finally
            {
                this.disposed = true;
            }
        }

        /// <summary>Dismisses the scope guard. Subsequent calls to <see cref="Dispose"/> will have no effect.</summary>
        /// <exception cref="ObjectDisposedException">The scope guard has already been disposed.</exception>
        public void Dismiss()
        {
            this.AssertNotDisposed();
            this.dismissed = true;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal ScopeGuard(T resource)
        {
            this.resource = resource;
            this.dismissed = false;
            this.disposed = false;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private readonly T resource;
        private bool dismissed;
        private bool disposed;

        private void AssertNotDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(this.ToString());
            }
        }
    }
}
