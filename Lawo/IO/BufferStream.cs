////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright 2008-2012 Andreas Huber Doenni, original from http://phuse.codeplex.com.
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.IO
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>Provides common functionality for streams that use buffers to read from and write to an underlying
    /// stream.</summary>
    /// <remarks>
    /// <para>A concrete subclass should implement the abstract
    /// <see cref="Stream.ReadAsync(byte[], int, int, CancellationToken)"/> method in terms of the
    /// <see cref="Buffer.this"/>, <see cref="IO.ReadBuffer.Index"/>, <see cref="IO.ReadBuffer.Count"/> and
    /// <see cref="IO.ReadBuffer.FillAsync(int, CancellationToken)"/> members of the object accessible through the
    /// <see cref="ReadBuffer"/> property. The abstract
    /// <see cref="Stream.WriteAsync(byte[], int, int, CancellationToken)"/> method should be implemented in terms of
    /// the <see cref="Buffer.this"/>, <see cref="IO.WriteBuffer.Count"/> and <see cref="IO.WriteBuffer.Flush"/> members of
    /// the object accessible through the <see cref="WriteBuffer"/> property.</para>
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Type derives from Stream, CA bug?")]
    public abstract class BufferStream : NonSeekableStream
    {
        /// <inheritdoc/>
        public sealed override bool CanRead => this.ReadBuffer != null;

        /// <inheritdoc/>
        public override bool CanWrite => this.WriteBuffer != null;

        /// <summary>Asynchronously flushes the write buffer and then disposes the underlying stream.</summary>
        public override async Task DisposeAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (!this.IsDisposed)
                {
                    await FlushAsync(cancellationToken).ConfigureAwait(false);
                    await base.DisposeAsync(cancellationToken).ConfigureAwait(false);
                }
            }
            finally
            {
                this.ReadBuffer = null;
                this.WriteBuffer = null;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Initializes a new instance of the <see cref="BufferStream"/> class.</summary>
        /// <remarks>Pass <c>null</c> for one of the buffers to create a stream that only supports reading or writing.</remarks>
        protected BufferStream(ReadBuffer readBuffer, WriteBuffer writeBuffer)
        {
            this.ReadBuffer = readBuffer;
            this.WriteBuffer = writeBuffer;
        }

        /// <summary>Gets a value indicating whether <see cref="Stream.Dispose()"/> has been called.</summary>
        protected bool IsDisposed => (this.ReadBuffer == null) && (this.WriteBuffer == null);

        /// <summary>Gets a reference to the read buffer.</summary>
        protected ReadBuffer ReadBuffer { get; private set; }

        /// <summary>Gets a reference to the write buffer.</summary>
        protected WriteBuffer WriteBuffer { get; private set; }

        /// <summary>Flushes the write buffer and then disposes the underlying stream.</summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Dispose() must never throw.")]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && !this.IsDisposed)
                {
                    this.Flush();
                }
            }
            catch
            {
            }
            finally
            {
                this.ReadBuffer = null;
                this.WriteBuffer = null;
                base.Dispose(disposing);
            }
        }
    }
}
