////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright 2008-2012 Andreas Huber Doenni, original from http://phuse.codeplex.com.
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.IO
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>Overrides <see cref="Stream"/> members associated with seeking so that they throw
    /// <see cref="NotSupportedException"/>.</summary>
    /// <remarks>
    /// <para><b>Thread Safety</b>: Any public static members of this type are thread safe. Any instance members are not
    /// guaranteed to be thread safe.</para>
    /// </remarks>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Type derives from Stream, CA bug?")]
    public abstract class NonSeekableStream : Stream
    {
        private static readonly Task Completed = Task.FromResult(false);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Asynchronously disposes resources.</summary>
        [CLSCompliant(false)]
        public virtual Task DisposeAsync(CancellationToken cancellationToken)
        {
            return Completed;
        }

        /// <summary>See <see cref="Stream.CanRead"/></summary>
        public override bool CanRead
        {
            get { return false; }
        }

        /// <summary>See <see cref="Stream.Read"/></summary>
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw CreateNotSupportedException();
        }

        /// <summary>See <see cref="Stream.CanWrite"/></summary>
        public override bool CanWrite
        {
            get { return false; }
        }

        /// <summary>See <see cref="Stream.Write"/></summary>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw CreateNotSupportedException();
        }

        /// <summary>See <see cref="Stream.CanSeek"/>.</summary>
        /// <value>Always <c>false</c>, even if the underlying stream supports seeking.</value>
        public sealed override bool CanSeek
        {
            get { return false; }
        }

        /// <summary>See <see cref="Stream.Length"/>.</summary>
        /// <exception cref="NotSupportedException">Thrown with each call.</exception>
        public sealed override long Length
        {
            get { throw CreateNotSupportedException(); }
        }

        /// <summary>See <see cref="Stream.SetLength"/>.</summary>
        /// <exception cref="NotSupportedException">Thrown with each call.</exception>
        public sealed override void SetLength(long value)
        {
            throw CreateNotSupportedException();
        }

        /// <summary>See <see cref="Stream.Position"/>.</summary>
        /// <exception cref="NotSupportedException">Thrown with each get or set operation.</exception>
        public sealed override long Position
        {
            get { throw CreateNotSupportedException(); }
            set { throw CreateNotSupportedException(); }
        }

        /// <summary>See <see cref="Stream.Seek"/>..</summary>
        /// <exception cref="NotSupportedException">Thrown with each call.</exception>
        public sealed override long Seek(long offset, SeekOrigin origin)
        {
            throw CreateNotSupportedException();
        }

        /// <summary>See <see cref="Stream.Flush"/>.</summary>
        /// <remarks>This method does nothing, derived classes should override as necessary.</remarks>
        public override void Flush()
        {
        }

        /// <summary>See <see cref="Stream.FlushAsync(CancellationToken)"/>.</summary>
        /// <remarks>This method does nothing, derived classes should override as necessary.</remarks>
        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return Completed;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Checks whether the object has been disposed and if so throws <see cref="ObjectDisposedException"/>.
        /// </summary>
        protected void AssertNotDisposed()
        {
            if (!this.CanRead && !this.CanWrite)
            {
                throw new ObjectDisposedException(this.ToString());
            }
        }

        /// <summary>Creates the exception thrown by all unsupported members.</summary>
        protected static Exception CreateNotSupportedException()
        {
            return new NotSupportedException("This operation is not supported.");
        }
    }
}
