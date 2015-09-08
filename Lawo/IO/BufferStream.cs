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

    /// <summary>Provides common functionality for streams that use buffers to read from and write to an underlying
    /// stream.</summary>
    /// <remarks>
    /// <para>A concrete subclass should implement the abstract <see cref="Stream.Read(byte[], int, int)"/> method in
    /// terms of the <see cref="M:ReadBuffer.Buffer"/>, <see cref="M:ReadBuffer.Offset"/>,
    /// <see cref="M:ReadBuffer.Count"/> and <see cref="M:ReadBuffer.FillBuffer"/> members of the
    /// <see cref="M:ReadBuffer"/> object. The abstract <see cref="Stream.Write(byte[], int, int)"/> method should be
    /// implemented in terms of the <see cref="M:WriteBuffer.Buffer"/>, <see cref="M:WriteBuffer.Offset"/> and
    /// <see cref="M:WriteBuffer.Flush"/> members of the <see cref="M:WriteBuffer"/> object.</para>
    /// <para><b>Thread Safety</b>: Any public static members of this type are thread safe. Any instance members are not
    /// guaranteed to be thread safe.</para>
    /// </remarks>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Type derives from Stream, CA bug?")]
    public abstract class BufferStream : NonSeekableStream
    {
        private ReadBuffer readBuffer;
        private WriteBuffer writeBuffer;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Asynchronously flushes the write buffer and then disposes the underlying stream.</summary>
        [CLSCompliant(false)]
        public override async Task DisposeAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (!this.IsDisposed)
                {
                    await this.FlushAsync(cancellationToken);
                    await base.DisposeAsync(cancellationToken);
                }
            }
            finally
            {
                this.readBuffer = null;
                this.writeBuffer = null;
            }
        }

        /// <summary>See <see cref="Stream.CanRead"/>.</summary>
        public sealed override bool CanRead
        {
            get { return this.readBuffer != null; }
        }

        /// <summary>See <see cref="Stream.CanWrite"/>.</summary>
        public override bool CanWrite
        {
            get { return this.writeBuffer != null; }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Initializes a new instance of the <see cref="BufferStream"/> class.</summary>
        /// <remarks>Pass <c>null</c> for one of the buffers to create a stream that only supports reading or writing.</remarks>
        protected BufferStream(ReadBuffer readBuffer, WriteBuffer writeBuffer)
        {
            this.readBuffer = readBuffer;
            this.writeBuffer = writeBuffer;
        }

        /// <summary>Gets a value indicating whether <see cref="Stream.Dispose()"/> has been called.</summary>
        protected bool IsDisposed
        {
            get { return (this.readBuffer == null) && (this.writeBuffer == null); }
        }

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
                this.readBuffer = null;
                this.writeBuffer = null;
                base.Dispose(disposing);
            }
        }

        /// <summary>Gets a reference to the read buffer.</summary>
        protected ReadBuffer ReadBuffer
        {
            get { return this.readBuffer; }
        }

        /// <summary>Gets a reference to the write buffer.</summary>
        protected WriteBuffer WriteBuffer
        {
            get { return this.writeBuffer; }
        }
    }
}
