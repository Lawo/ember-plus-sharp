////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.S101
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using Lawo.IO;
    using Lawo.Threading.Tasks;

    /// <summary>Represents a writer that provides the means to generate S101-encoded messages.</summary>
    /// <remarks>
    /// <para>See
    /// <a href="http://ember-plus.googlecode.com/svn/trunk/documentation/Ember+%20Documentation.pdf">Ember+
    /// Specification</a>, Chapter "Message Framing".</para>
    /// <para><b>Thread Safety</b>: Any public static members of this type are thread safe. Any instance members are not
    /// guaranteed to be thread safe.</para>
    /// </remarks>
    public sealed class S101Writer
    {
        private readonly TaskSingleton taskSingleton = new TaskSingleton();
        private readonly WriteBuffer writeBuffer;
        private MessageEncodingStream stream;
        private bool disposed;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Initializes a new instance of the <see cref="S101Writer"/> class by calling
        /// <see cref="S101Writer(WriteAsyncCallback, int)"/>(<paramref name="writeAsync"/>, 8192).</summary>
        [CLSCompliant(false)]
        public S101Writer(WriteAsyncCallback writeAsync) : this(writeAsync, Constants.PhysicalStreamBufferSize)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="S101Writer"/> class.</summary>
        /// <param name="writeAsync">The method that is called when bytes need to be written asynchronously to the
        /// S101-encoded sink.</param>
        /// <param name="bufferSize">The size of the internal buffer in bytes.</param>
        /// <exception cref="ArgumentNullException"><paramref name="writeAsync"/> equals <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="bufferSize"/> is 0 or negative.</exception>
        /// <remarks>This method accepts callbacks rather than the usual <see cref="Stream"/> object, so that it can
        /// also be used for network APIs for which .NET does not offer <see cref="Stream"/> subclasses, as is the
        /// case for System.Net.Sockets.Socket objects where Socket.SocketType does not equal SocketType.Stream.
        /// </remarks>
        [CLSCompliant(false)]
        public S101Writer(WriteAsyncCallback writeAsync, int bufferSize)
        {
            this.writeBuffer = new WriteBuffer(writeAsync, bufferSize);
        }

        /// <summary>Asynchronously releases all resources used by the current instance of the <see cref="S101Writer"/>
        /// class.</summary>
        /// <exception cref="Exception">An exception was thrown from the callback passed to the constructor, see
        /// <see cref="Exception.Message"/> for more information.</exception>
        /// <exception cref="InvalidOperationException">The <see cref="Task.IsCompleted"/> property is <c>false</c> for
        /// the <see cref="Task"/> object returned by a previously called async method.</exception>
        [CLSCompliant(false)]
        public Task DisposeAsync(CancellationToken cancellationToken)
        {
            return this.taskSingleton.Execute(() => this.DisposeCoreAsync(cancellationToken));
        }

        /// <summary>Asynchronously writes <paramref name="message"/>.</summary>
        /// <param name="message">The message to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// <para>A <see cref="Stream"/> instance that can be used to write the payload of
        /// <paramref name="message"/>, if <paramref name="message"/> can have a payload; otherwise <c>null</c>. Call
        /// <see cref="NonSeekableStream.DisposeAsync"/> after writing the payload.</para>
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="message"/> equals <c>null</c>.</exception>
        /// <exception cref="Exception">An exception was thrown from the callback passed to the constructor, see
        /// <see cref="Exception.Message"/> for more information.</exception>
        /// <exception cref="InvalidOperationException"><list type="bullet">
        /// <item><see cref="Stream.Dispose()"/> has not been called on the payload stream of the previous message, or
        /// </item>
        /// <item>the <see cref="Task.IsCompleted"/> property is <c>false</c> for the <see cref="Task"/> object returned
        /// by a previously called async method.</item>
        /// </list></exception>
        /// <exception cref="ObjectDisposedException"><see cref="DisposeAsync"/> has been called.</exception>
        /// <remarks>
        /// <para>If necessary, the message plus payload is automatically partitioned into multiple packets such that the
        /// unencoded length of each packet does not exceed 1024 bytes.</para>
        /// </remarks>
        [CLSCompliant(false)]
        public Task<NonSeekableStream> WriteMessageAsync(S101Message message, CancellationToken cancellationToken)
        {
            return this.taskSingleton.Execute(() => this.WriteMessageCoreAsync(message, cancellationToken));
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private async Task DisposeCoreAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (this.stream != null)
                {
                    await this.stream.DisposeAsync(cancellationToken);
                }
            }
            finally
            {
                this.disposed = true;
                this.stream = null;
            }
        }

        private async Task<NonSeekableStream> WriteMessageCoreAsync(
            S101Message message, CancellationToken cancellationToken)
        {
            this.AssertNotDisposed();

            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            if ((this.stream != null) && this.stream.CanWrite)
            {
                throw new InvalidOperationException(
                    "DisposeAsync() has not been called on the payload stream of the previous message.");
            }

            this.stream = await MessageEncodingStream.CreateAsync(this.writeBuffer, message, cancellationToken);

            if (!message.CanHavePayload)
            {
                await this.stream.DisposeAsync(cancellationToken);
                this.stream = null;
            }

            return this.stream;
        }

        private void AssertNotDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }
    }
}
