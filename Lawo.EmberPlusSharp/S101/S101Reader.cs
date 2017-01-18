////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.S101
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using IO;
    using Threading.Tasks;

    /// <summary>Represents a reader that provides access to S101-encoded messages and their payload.</summary>
    /// <remarks>See the <i>"Ember+ Specification"</i><cite>Ember+ Specification</cite>, chapter "Message Framing".
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    public sealed class S101Reader
    {
        /// <summary>Initializes a new instance of the <see cref="S101Reader"/> class by calling
        /// <see cref="S101Reader(ReadAsyncCallback, int)">S101Reader(<paramref name="readAsync"/>, 8192)</see>.
        /// </summary>
        [CLSCompliant(false)]
        public S101Reader(ReadAsyncCallback readAsync)
            : this(readAsync, Constants.PhysicalStreamBufferSize)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="S101Reader"/> class.</summary>
        /// <param name="readAsync">The method that is called when bytes need to be read asynchronously from the
        /// S101-encoded source.</param>
        /// <param name="bufferSize">The size of the internal buffer in bytes.</param>
        /// <exception cref="ArgumentNullException"><paramref name="readAsync"/> equals <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="bufferSize"/> is 0 or negative.</exception>
        /// <remarks>This method accepts a callback rather than the usual <see cref="Stream"/> object, so that it can
        /// also be used for network APIs for which .NET does not offer <see cref="Stream"/> subclasses, as is the case
        /// for System.Net.Sockets.Socket objects where Socket.SocketType does not equal SocketType.Stream.</remarks>
        [CLSCompliant(false)]
        public S101Reader(ReadAsyncCallback readAsync, int bufferSize)
        {
            this.readBuffer = new ReadBuffer(readAsync, bufferSize);
        }

        /// <summary>Occurs when an out-of-frame byte has been received.</summary>
        public event EventHandler<OutOfFrameByteReceivedEventArgs> OutOfFrameByteReceived;

        /// <summary>Gets the current message.</summary>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item><see cref="ReadAsync"/> has never been called, or</item>
        /// <item>the last call to <see cref="ReadAsync"/> returned <c>false</c> or threw an exception, or</item>
        /// <item>the <see cref="Task.IsCompleted"/> property is <c>false</c> for the <see cref="Task"/> object returned
        /// by a previously called async method.</item>
        /// </list></exception>
        /// <exception cref="ObjectDisposedException"><see cref="DisposeAsync"/> has been called.</exception>
        public S101Message Message
        {
            get
            {
                this.taskSingleton.AssertTaskIsCompleted();
                this.AssertRead();
                return this.stream.Message;
            }
        }

        /// <summary>Gets a <see cref="NonSeekableStream"/> instance that can be used to read the payload of the current
        /// message.</summary>
        /// <remarks>
        /// <para>Call any of the Read methods on the returned <see cref="NonSeekableStream"/> object to obtain the
        /// payload.</para>
        /// <para>If a message contains multiple packets, their payload is automatically joined such that it can be read
        /// through the returned stream as if the message consisted of only one packet.</para>
        /// <para>If the current message does not have a payload or if the end of the payload has been reached, return
        /// values from Read method calls will indicate that the end of the stream has been reached.</para>
        /// <para>Read methods of the returned <see cref="Stream"/> object will throw <see cref="S101Exception"/> to
        /// signal parsing errors, see <see cref="Exception.Message"/> for more information.</para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item><see cref="ReadAsync"/> has never been called, or</item>
        /// <item>the last call to <see cref="ReadAsync"/> returned <c>false</c> or threw an exception, or</item>
        /// <item>the <see cref="Task.IsCompleted"/> property is <c>false</c> for the <see cref="Task"/> object returned
        /// by a previously called async method.</item>
        /// </list></exception>
        /// <exception cref="ObjectDisposedException"><see cref="DisposeAsync"/> has been called.</exception>
        public NonSeekableStream Payload
        {
            get
            {
                this.taskSingleton.AssertTaskIsCompleted();
                this.AssertRead();
                return this.stream;
            }
        }

        /// <summary>Gets a value indicating whether another message is available.</summary>
        /// <exception cref="InvalidOperationException">The <see cref="Task.IsCompleted"/> property is <c>false</c> for
        /// the <see cref="Task"/> object returned by a previously called async method.</exception>
        public bool IsAnotherMessageAvailable
        {
            get
            {
                this.taskSingleton.AssertTaskIsCompleted();
                return this.readBuffer.Index < this.readBuffer.Count;
            }
        }

        /// <summary>Asynchronously releases all resources used by the current instance of the <see cref="S101Reader"/>
        /// class.</summary>
        /// <exception cref="Exception">An exception was thrown from the callback passed to the constructor, see
        /// <see cref="Exception.Message"/> for more information.</exception>
        /// <exception cref="InvalidOperationException">The <see cref="Task.IsCompleted"/> property is <c>false</c> for
        /// the <see cref="Task"/> object returned by a previously called async method.</exception>
        [CLSCompliant(false)]
        public Task DisposeAsync(CancellationToken cancellationToken) =>
            this.taskSingleton.Execute(() => this.DisposeCoreAsync(cancellationToken));

        /// <summary>Asynchronously reads the next message.</summary>
        /// <returns><c>true</c> if the next message was read successfully; <c>false</c> if there are no more messages
        /// to read.</returns>
        /// <exception cref="Exception">An exception was thrown from the callback passed to the constructor, see
        /// <see cref="Exception.Message"/> for more information.</exception>
        /// <exception cref="InvalidOperationException">The <see cref="Task.IsCompleted"/> property is <c>false</c> for
        /// the <see cref="Task"/> object returned by a previously called async method.</exception>
        /// <exception cref="ObjectDisposedException"><see cref="DisposeAsync"/> has been called.</exception>
        /// <exception cref="S101Exception">An error occurred while parsing the S101-encoded data, see
        /// <see cref="Exception.Message"/> for more information.</exception>
        /// <remarks>
        /// <para>When a <see cref="S101Reader"/> is first created and initialized, there is no information available.
        /// You must call <see cref="ReadAsync"/> to read the first message.</para>
        /// <para>Possibly unread payload of the previous message is automatically skipped.</para></remarks>
        [CLSCompliant(false)]
        public Task<bool> ReadAsync(CancellationToken cancellationToken) =>
            this.taskSingleton.Execute(() => this.ReadCoreAsync(cancellationToken));

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private readonly TaskSingleton taskSingleton = new TaskSingleton();
        private readonly ReadBuffer readBuffer;
        private readonly byte[] discardBuffer = new byte[Defaults.InMemoryStreamBufferSize];
        private MessageDecodingStream stream;
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

        private async Task<bool> ReadCoreAsync(CancellationToken cancellationToken)
        {
            this.AssertNotDisposed();

            if (this.stream != null)
            {
                await this.stream.DisposeAsync(cancellationToken);
            }

            this.stream = await MessageDecodingStream.CreateAsync(
                this.readBuffer, this.discardBuffer, this.OnOutOfFrameByteReceived, cancellationToken);
            return this.stream.Message != null;
        }

        private void OnOutOfFrameByteReceived(byte value) =>
            this.OutOfFrameByteReceived?.Invoke(this, new OutOfFrameByteReceivedEventArgs(value));

        private void AssertNotDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }

        private void AssertRead()
        {
            this.AssertNotDisposed();

            if (this.stream == null)
            {
                throw new InvalidOperationException("Read() has never been called.");
            }

            if (this.stream.Message == null)
            {
                throw new InvalidOperationException("The last call to Read() returned false or threw an exception.");
            }
        }
    }
}
