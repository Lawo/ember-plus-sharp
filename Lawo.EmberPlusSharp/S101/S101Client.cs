////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.S101
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using IO;
    using Threading;
    using Threading.Tasks;

    /// <summary>Provides methods to communicate with S101-encoded messages over a given connection.</summary>
    /// <remarks>
    /// <para>See the <i>"Ember+ Specification"</i><cite>Ember+ Specification</cite>, chapter "Message Framing".</para>
    /// <para>Automatically answers any message containing a <see cref="KeepAliveRequest"/> command with a message
    /// containing a <see cref="KeepAliveResponse"/> command.</para>
    /// <para>Automatically sends messages containing a <see cref="KeepAliveRequest"/> command according to the value
    /// passed to <see cref="S101Client(IDisposable, ReadAsyncCallback, WriteAsyncCallback, IS101Logger, int, int)"/>
    /// for the timeout parameter. If no bytes are received from the remote party for half the timeout period then a
    /// message containing a <see cref="KeepAliveRequest"/> is sent. If no message of any kind is received during the
    /// second half of the timeout period, the <see cref="ConnectionLost"/> event is raised with an
    /// <see cref="S101Exception"/>.</para>
    /// <para>This class <b>requires</b> that <see cref="SynchronizationContext.Current"/> returns a context that
    /// executes all continuations on a single thread. For an <see cref="S101Client"/> object constructed on the GUI
    /// thread of a Windows Forms, WPF or Windows Store App this already the case. Other environments, e.g. Console
    /// Applications, ASP.net applications or unit test environments either do not have a synchronization context
    /// (<see cref="SynchronizationContext.Current"/> equals <c>null</c>) or do not guarantee execution on a single
    /// thread. For such environments it is the responsibility of the client to set
    /// <see cref="SynchronizationContext.Current"/> appropriately before constructing a <see cref="S101Client"/>
    /// object. <see cref="AsyncPump.Run"/> provides an easy way to do that.</para>
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    public sealed class S101Client : IMonitoredConnection
    {
        /// <summary>Initializes a new instance of the <see cref="S101Client"/> class by calling
        /// <see cref="S101Client(IDisposable, ReadAsyncCallback, WriteAsyncCallback, IS101Logger)">S101Client(<paramref name="connection"/>,
        /// <paramref name="readAsync"/>, <paramref name="writeAsync"/>, null)</see>.</summary>
        /// <remarks>See <see cref="S101Client"/> remarks for more information.</remarks>
        [CLSCompliant(false)]
        public S101Client(IDisposable connection, ReadAsyncCallback readAsync, WriteAsyncCallback writeAsync)
            : this(connection, readAsync, writeAsync, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="S101Client"/> class by calling
        /// <see cref="S101Client(IDisposable, ReadAsyncCallback, WriteAsyncCallback, IS101Logger, int, int)">S101Client(<paramref name="connection"/>,
        /// <paramref name="readAsync"/>, <paramref name="writeAsync"/>, <paramref name="logger"/>, 3000, 8192)</see>.
        /// </summary>
        /// <remarks>See <see cref="S101Client"/> remarks for more information.</remarks>
        [CLSCompliant(false)]
        public S101Client(
            IDisposable connection, ReadAsyncCallback readAsync, WriteAsyncCallback writeAsync, IS101Logger logger)
            : this(connection, readAsync, writeAsync, logger, 3000, 8192)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="S101Client"/> class.</summary>
        /// <param name="connection">An object that represents an already established connection through which bytes
        /// are read and written. After a call to the <see cref="IDisposable.Dispose"/> method of this object, both
        /// <paramref name="readAsync"/> and <paramref name="writeAsync"/> must complete by throwing either an
        /// <see cref="ObjectDisposedException"/> or an <see cref="OperationCanceledException"/>.</param>
        /// <param name="readAsync">References the method to be called when bytes need to be read through the
        /// connection.</param>
        /// <param name="writeAsync">References the method to be called when bytes need to be written through the
        /// connection.</param>
        /// <param name="logger">The logger to log activity to, can be <c>null</c>.</param>
        /// <param name="timeout">The total amount of time, in milliseconds, the <see cref="S101Client"/> instance will
        /// wait before the connection is shut down due to a failure of the other party to respond. Specify -1 to wait
        /// indefinitely.</param>
        /// <param name="bufferSize">The size of the internal read and write buffers in bytes.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/>, <paramref name="readAsync"/> and/or
        /// <paramref name="writeAsync"/> equal <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="bufferSize"/> is 0 or negative and/or
        /// <paramref name="timeout"/> is less than -1.</exception>
        /// <exception cref="NotSupportedException">S101Client is not supported when SynchronizationContext.Current ==
        /// null.</exception>
        /// <remarks>See <see cref="S101Client"/> remarks for more information.</remarks>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", Justification = "Official class name.")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed in ReadLoop.")]
        [CLSCompliant(false)]
        public S101Client(
            IDisposable connection,
            ReadAsyncCallback readAsync,
            WriteAsyncCallback writeAsync,
            IS101Logger logger,
            int timeout,
            int bufferSize)
        {
            ReadAsyncCallback readAsyncWithLog;

            using (var connectionGuard = ScopeGuard.Create(connection))
            using (var loggerGuard = ScopeGuard.Create(logger))
            {
                if (SynchronizationContext.Current == null)
                {
                    throw new NotSupportedException(
                        "S101Client is not supported when SynchronizationContext.Current == null");
                }

                if (connection == null)
                {
                    throw new ArgumentNullException(nameof(connection));
                }

                if (readAsync == null)
                {
                    throw new ArgumentNullException(nameof(readAsync));
                }

                if (writeAsync == null)
                {
                    throw new ArgumentNullException(nameof(writeAsync));
                }

                if (timeout < -1)
                {
                    throw new ArgumentOutOfRangeException(nameof(timeout), "A number >= -1 is required.");
                }

                WriteAsyncCallback writeAsyncWithLog;

                if (logger == null)
                {
                    readAsyncWithLog = readAsync;
                    writeAsyncWithLog = writeAsync;
                }
                else
                {
                    const string Type = "RawData";

                    readAsyncWithLog =
                        async (b, o, c, t) =>
                        {
                            var read = await readAsync(b, o, c, t);
                            await this.logQueue.Enqueue(() => this.logger.LogData(Type, LogNames.Receive, b, o, read));
                            return read;
                        };

                    writeAsyncWithLog =
                        async (b, o, c, t) =>
                        {
                            await this.logQueue.Enqueue(() => this.logger.LogData(Type, LogNames.Send, b, o, c));
                            await writeAsync(b, o, c, t);
                        };
                }

                this.threadId = NativeMethods.GetCurrentThreadId();
                this.writer = new S101Writer(writeAsyncWithLog, bufferSize);
                this.logger = logger;
                this.timeout = timeout;
                connectionGuard.Dismiss();
                loggerGuard.Dismiss();
            }

            this.ReadLoop(connection, new S101Reader(readAsyncWithLog, bufferSize));
        }

        /// <summary>Occurs when an out-of-frame byte has been received.</summary>
        public event EventHandler<OutOfFrameByteReceivedEventArgs> OutOfFrameByteReceived;

        /// <summary>Occurs when the client has received the full payload of a message with an <see cref="EmberData"/>
        /// command.</summary>
        public event EventHandler<MessageReceivedEventArgs> EmberDataReceived;

        /// <summary>Occurs when the connection to the provider has been lost.</summary>
        /// <remarks>
        /// <para>This event is raised in the following situations:
        /// <list type="bullet">
        /// <item>There was a communication error, or</item>
        /// <item>The remote host has failed to answer a <see cref="KeepAliveRequest"/>, or</item>
        /// <item>The remote host has gracefully shutdown its connection, or</item>
        /// <item>Client code has called <see cref="Dispose"/>.</item>
        /// </list>
        /// For the first two cases <see cref="ConnectionLostEventArgs.Exception"/> indicates the source of the error.
        /// For the last two cases <see cref="ConnectionLostEventArgs.Exception"/> is <c>null</c>.</para>
        /// </remarks>
        public event EventHandler<ConnectionLostEventArgs> ConnectionLost;

        /// <summary>Gets or sets the value to set the <see cref="S101Message.Slot"/> property to for a message
        /// containing a <see cref="KeepAliveRequest"/> command.</summary>
        /// <value>The slot to set. The default is <c>0x00</c>.</value>
        /// <exception cref="InvalidOperationException">The getter or setter was called from a thread other than the one
        /// that executed the constructor.</exception>
        /// <exception cref="ObjectDisposedException"><see cref="Dispose"/> has been called or the
        /// <see cref="ConnectionLost"/> event has been raised.</exception>
        public byte KeepAliveRequestSlot
        {
            get
            {
                this.AssertPreconditions();
                return this.keepAliveRequestSlot;
            }

            set
            {
                this.AssertPreconditions();
                this.keepAliveRequestSlot = value;
            }
        }

        /// <summary>Sends <paramref name="value"/> as an out-of-frame byte.</summary>
        /// <param name="value">The byte to write.</param>
        /// <exception cref="ArgumentException"><paramref name="value"/> equals <c>0xFE</c>.</exception>
        public Task SendOutOfFrameByteAsync(byte value)
        {
            this.AssertPreconditions();
            return this.SendOutOfFrameByteCoreAsync(value);
        }

        /// <summary>Calls
        /// <see cref="SendMessageAsync(S101Message, byte[])">SendMessageAsync(<paramref name="message"/>, null)</see>.
        /// </summary>
        public Task SendMessageAsync(S101Message message) => this.SendMessageAsync(message, null);

        /// <summary>Sends <paramref name="message"/> followed by <paramref name="payload"/>.</summary>
        /// <param name="message">The message to send.</param>
        /// <param name="payload">The payload to send after the message. Must be equal to <c>null</c> if
        /// <see cref="S101Message.Command"/> does not allow for a payload. Must reference an appropriate payload if
        /// <see cref="S101Message.Command"/> requires a payload.</param>
        /// <exception cref="ArgumentException"><list type="bullet">
        /// <item>The <see cref="S101Message.Command"/> property of <paramref name="message"/> is of a type that
        /// requires a payload and <paramref name="payload"/> equals <c>null</c>, or</item>
        /// <item>The <see cref="S101Message.Command"/> property of <paramref name="message"/> is of a type that does
        /// not allow for a payload and <paramref name="payload"/> is not equal to <c>null</c>.</item>
        /// </list></exception>
        /// <exception cref="Exception">An exception was thrown from one of the callbacks passed to the constructor, see
        /// <see cref="Exception.Message"/> for more information.</exception>
        /// <exception cref="InvalidOperationException">This method was called from a thread other than the one that
        /// executed the constructor.</exception>
        /// <exception cref="ObjectDisposedException"><see cref="Dispose"/> has been called or the
        /// <see cref="ConnectionLost"/> event has been raised.</exception>
        /// <exception cref="OperationCanceledException"><see cref="Dispose"/> has been called or the
        /// <see cref="ConnectionLost"/> event has been raised.</exception>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1625:Element documentation must not be copied and pasted", Justification = "Intended, both exceptions can be thrown under the same circumstances.")]
        public Task SendMessageAsync(S101Message message, byte[] payload)
        {
            this.AssertPreconditions();
            return this.SendMessageCoreAsync(message, payload);
        }

        /// <summary>See <see cref="IDisposable.Dispose"/>.</summary>
        /// <remarks>Cancels all communication currently in progress and calls <see cref="IDisposable.Dispose"/> on the
        /// connection object passed to the constructor.</remarks>
        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", Justification = "All fields are disposed in ReadLoop.")]
        public void Dispose() => this.DisposeCore(true);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static async Task Delay(Task task, int milliseconds)
        {
            await task;
            await Task.Delay(milliseconds);
        }

        private static async Task<byte[]> GetPayload(S101Reader reader, byte[] buffer, CancellationToken token)
        {
            if (reader.Message.Command is EmberData)
            {
                var source = reader.Payload;

                using (var destination = new MemoryStream())
                {
                    int read;

                    while ((read = await source.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
                    {
                        destination.Write(buffer, 0, read);
                    }

                    await source.DisposeAsync(token);
                    return destination.ToArray();
                }
            }
            else
            {
                return null;
            }
        }

        private readonly WorkQueue logQueue = new WorkQueue();
        private readonly TaskQueue sendQueue = new TaskQueue();
        private readonly CancellationTokenSource source = new CancellationTokenSource();
        private readonly uint threadId;
        private readonly S101Writer writer;
        private readonly IS101Logger logger;
        private readonly int timeout;
        private byte keepAliveRequestSlot;

        private void AssertPreconditions()
        {
            if (this.source.IsCancellationRequested)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }

            if (this.threadId != NativeMethods.GetCurrentThreadId())
            {
                throw new InvalidOperationException(
                    "Accessed a S101Client object from a thread other than the one that executed the constructor.");
            }
        }

        private async void ReadLoop(IDisposable connection, S101Reader reader)
        {
            var disposed = new TaskCompletionSource<bool>();
            var cancellationFailed = Delay(disposed.Task, 250);
            this.source.Token.Register(() => disposed.SetResult(true));

            await this.EnqueueLogOperation(() => this.logger.LogEvent("StartingReadLoop"));
            reader.OutOfFrameByteReceived += this.OnOutOfFrameByteReceived;
            Exception exception;
            bool remote = false;

            try
            {
                var buffer = new byte[1024];

                while (await this.ReadWithTimeoutAsync(connection, reader, cancellationFailed))
                {
                    await this.ProcessMessage(reader, buffer);
                }

                remote = true;
                exception = null;
            }
            catch (OperationCanceledException)
            {
                exception = null;
            }
            catch (ObjectDisposedException)
            {
                exception = null;
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                reader.OutOfFrameByteReceived -= this.OnOutOfFrameByteReceived;
                this.DisposeCore(false);
                connection?.Dispose();
                this.source.Dispose();
            }

            try
            {
                Action logAction;

                if (exception == null)
                {
                    logAction =
                        () => this.logger.LogEvent(remote ? "RemoteGracefulDisconnect" : "LocalGracefulDisconnect");
                }
                else
                {
                    logAction = () => this.logger.LogException(LogNames.Receive, exception);
                }

                // We're deliberately not awaiting this task, so that the Dispose call will be enqueued even if this
                // one fails with an exception.
                this.EnqueueLogOperation(logAction).Ignore();
                await this.EnqueueLogOperation(() => this.logger.Dispose());
                await cancellationFailed;
            }
            finally
            {
                this.OnEvent(this.ConnectionLost, new ConnectionLostEventArgs(exception));
            }
        }

        private void DisposeCore(bool log)
        {
            if (!this.source.IsCancellationRequested)
            {
                if (log)
                {
                    // We're deliberately not awaiting this task, it will be implicitly awaited by following logging
                    // operations.
                    this.EnqueueLogOperation(() => this.logger.LogEvent("StoppingReadLoop")).Ignore();
                }

                this.source.Cancel();
            }
        }

        private async Task SendMessageCoreAsync(S101Message message, byte[] payload)
        {
            await this.EnqueueLogOperation(() => this.logger.LogMessage(LogNames.Send, message, payload));

            await this.sendQueue.Enqueue(
                async () =>
                {
                    var payloadStream = await this.writer.WriteMessageAsync(message, this.source.Token);

                    if ((payload == null) != (payloadStream == null))
                    {
                        throw new ArgumentException(
                            "The payload requirements of the command of the passed message do not match the passed payload.",
                            nameof(payload));
                    }

                    if (payload != null)
                    {
                        await payloadStream.WriteAsync(payload, 0, payload.Length, this.source.Token);
                        await payloadStream.DisposeAsync(this.source.Token);
                    }
                });
        }

        private async Task SendOutOfFrameByteCoreAsync(byte value)
        {
            await this.EnqueueLogOperation(() => this.logger.LogData(LogNames.OutOfFrameByte, LogNames.Send, new[] { value }, 0, 1));
            await this.sendQueue.Enqueue(() => this.writer.WriteOutOfFrameByteAsync(value, this.source.Token));
        }

        private async Task<bool> ReadWithTimeoutAsync(
            IDisposable connection, S101Reader reader, Task cancellationFailed)
        {
            int timeoutCount = 0;
            var timeoutHalf = this.timeout >= 0 ? this.timeout / 2 : this.timeout;
            var readTask = reader.ReadAsync(this.source.Token);
            Task timeoutTask;

            // In a perfect world, the cancellationFailed business would not be necessary, because readTask should
            // complete immediately when a cancellation is requested. In the real world however, such "rarely" used
            // classes as NetworkStream do not support cancellation, see
            // http://stackoverflow.com/questions/12421989/networkstream-readasync-with-a-cancellation-token-never-cancels
            while (
                await Task.WhenAny(readTask, timeoutTask = Task.Delay(timeoutHalf), cancellationFailed) == timeoutTask)
            {
                switch (++timeoutCount)
                {
                    case 1:
                        await this.SendMessageCoreAsync(
                            new S101Message(this.keepAliveRequestSlot, new KeepAliveRequest()), null);
                        break;
                    case 2:
                        await this.EnqueueLogOperation(() => this.logger.LogEvent("TimeoutExpired"));
                        this.Dispose();
                        break;
                }
            }

            if (cancellationFailed.IsCompleted)
            {
                await this.EnqueueLogOperation(() => this.logger.LogEvent("CancellationFailed"));

                // For IO objects that do not support cancellation with CancellationToken, the recommended practice is
                // to simply dispose and then wait for the async operations to complete.
                connection?.Dispose();
            }

            try
            {
                return await readTask;
            }
            catch (Exception ex)
            {
                if (((ex is OperationCanceledException) || (ex is ObjectDisposedException)) && (timeoutCount > 1))
                {
                    throw new S101Exception(
                        "The remote host has failed to answer a KeepAliveRequest within half the timeout period.", ex);
                }
                else
                {
                    throw;
                }
            }
        }

        private async void OnOutOfFrameByteReceived(object sender, OutOfFrameByteReceivedEventArgs e)
        {
            await this.EnqueueLogOperation(
                () => this.logger.LogData(LogNames.OutOfFrameByte, LogNames.Receive, new[] { e.Value }, 0, 1));
            this.OnEvent(this.OutOfFrameByteReceived, e);
        }

        private void OnEvent<TEventArgs>(EventHandler<TEventArgs> handler, TEventArgs args)
            where TEventArgs : EventArgs
        {
            handler?.Invoke(this, args);
        }

        private async Task ProcessMessage(S101Reader reader, byte[] buffer)
        {
            var message = reader.Message;
            var payload = await GetPayload(reader, buffer, this.source.Token);
            await this.EnqueueLogOperation(() => this.logger.LogMessage(LogNames.Receive, message, payload));

            if (message.Command is EmberData)
            {
                this.OnEvent(
                    this.EmberDataReceived,
                    new MessageReceivedEventArgs(message, payload, reader.IsAnotherMessageAvailable));
            }
            else if (message.Command is KeepAliveRequest)
            {
                await this.SendMessageAsync(new S101Message(message.Slot, new KeepAliveResponse()));
            }

            // We don't need to do anything with a KeepAliveResponse, because it has already made sure that no timeout
            // exception has been thrown.
        }

        private Task EnqueueLogOperation(Action logOperation)
        {
            if (this.logger == null)
            {
                return Task.FromResult(false);
            }
            else
            {
                return this.logQueue.Enqueue(logOperation);
            }
        }
    }
}
