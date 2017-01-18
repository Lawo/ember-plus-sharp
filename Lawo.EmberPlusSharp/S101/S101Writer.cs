////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.S101
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using IO;
    using Threading.Tasks;

    /// <summary>Represents a writer that provides the means to generate S101-encoded messages.</summary>
    /// <remarks>See the <i>"Ember+ Specification"</i><cite>Ember+ Specification</cite>, chapter "Message Framing".
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    public sealed class S101Writer
    {
        /// <summary>Initializes a new instance of the <see cref="S101Writer"/> class by calling
        /// <see cref="S101Writer(WriteAsyncCallback, int)">S101Writer(<paramref name="writeAsync"/>, 8192)</see>.</summary>
        [CLSCompliant(false)]
        public S101Writer(WriteAsyncCallback writeAsync)
            : this(writeAsync, Constants.PhysicalStreamBufferSize)
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
        public Task DisposeAsync(CancellationToken cancellationToken) =>
            this.taskSingleton.Execute(() => this.DisposeCoreAsync(cancellationToken));

        /// <summary>Writes <paramref name="value"/> as an out-of-frame byte.</summary>
        /// <param name="value">The byte to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <exception cref="ArgumentException"><paramref name="value"/> equals <c>0xFE</c>.</exception>
        public Task WriteOutOfFrameByteAsync(byte value, CancellationToken cancellationToken) =>
            this.taskSingleton.Execute(() => this.WriteOutOfFrameByteCoreAsync(value, cancellationToken));

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

        private readonly TaskSingleton taskSingleton = new TaskSingleton();
        private readonly WriteBuffer writeBuffer;
        private MessageEncodingStream stream;
        private bool disposed;

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
                throw new ArgumentNullException(nameof(message));
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

        private async Task WriteOutOfFrameByteCoreAsync(byte value, CancellationToken cancellationToken)
        {
            this.AssertNotDisposed();

            if (value == Frame.BeginOfFrame)
            {
                var message = string.Format(
                    CultureInfo.InvariantCulture, "A value not equal to {0:X2} is required.", Frame.BeginOfFrame);
                throw new ArgumentException(message, nameof(value));
            }

            if ((this.stream == null) || !this.stream.CanWrite)
            {
                await this.writeBuffer.ReserveAsync(1, cancellationToken);
                this.writeBuffer[this.writeBuffer.Count++] = value;
                await this.writeBuffer.FlushAsync(cancellationToken);
            }
            else
            {
                await this.stream.WriteOutOfFrameByteAsync(value, cancellationToken);
            }
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
