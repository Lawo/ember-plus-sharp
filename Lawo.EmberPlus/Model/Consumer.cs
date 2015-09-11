////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Ember;
    using IO;
    using S101;

    /// <summary>Communicates with a provider to create and maintain a <typeparamref name="TRoot"/> object.</summary>
    /// <typeparam name="TRoot">The type of the root object to be filled and maintained.</typeparam>
    /// <threadsafety static="true" instance="false"/>
    public sealed class Consumer<TRoot> : IMonitoredConnection, IInvocationCollection where TRoot : Root<TRoot>
    {
        private static readonly EmberData EmberDataCommand = new EmberData(0x01, 0x0A, 0x02);

        private readonly TRoot root = Root<TRoot>.Construct(new Context(null, 0, string.Empty));
        private readonly ReceiveQueue receiveQueue = new ReceiveQueue();
        private readonly Dictionary<int, IInvocationResult> pendingInvocations =
            new Dictionary<int, IInvocationResult>();

        private readonly S101Client client;
        private readonly int queryChildrenTimeout;
        private readonly S101Message emberDataMessage;
        private int autoSendInterval = 100;
        private CancellationTokenSource autoSendDelayCancellationSource;
        private TaskCompletionSource<bool> hasChangesSetSource;
        private int lastInvocationId;
        private bool disposed;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets the root object.</summary>
        public TRoot Root
        {
            get { return this.root; }
        }

        /// <summary>Gets or sets the minimal amount of time, in milliseconds, the consumer will wait after
        /// automatically calling <see cref="SendAsync"/> before it will automatically call it again.</summary>
        /// <value>The interval in milliseconds. Default is 100. Set to <see cref="Timeout.Infinite"/> to never send
        /// changes automatically.</value>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is less than -1.</exception>
        public int AutoSendInterval
        {
            get
            {
                return this.autoSendInterval;
            }

            set
            {
                if (value < Timeout.Infinite)
                {
                    throw new ArgumentOutOfRangeException("value", "Must be >= -1.");
                }

                this.autoSendInterval = value;
                this.CancelAutoSendDelay();
            }
        }

        /// <summary>Calls <see cref="SendAsync"/>.</summary>
        [Obsolete("Call SendAsync instead.")]
        public Task SendChangesAsync()
        {
            return this.SendAsync();
        }

        /// <summary>Asynchronously sends the locally applied changes and invocations to the provider.</summary>
        /// <exception cref="Exception">An exception was thrown from one of the callbacks passed to the
        /// <see cref="S101Client"/> constructor, see <see cref="Exception.Message"/> for more information.</exception>
        /// <exception cref="InvalidOperationException">This method was called from a thread other than the one that
        /// executed <see cref="CreateAsync(S101Client, int, byte)"/>.</exception>
        /// <exception cref="ObjectDisposedException"><see cref="Dispose"/> has been called or the connection has been
        /// lost.</exception>
        /// <exception cref="OperationCanceledException"><see cref="Dispose"/> has been called or the connection has
        /// been lost.</exception>
        public async Task SendAsync()
        {
            if (this.root.HasChanges)
            {
                MemoryStream stream;

                // TODO: Reuse MemoryStream and EmberWriter for all outgoing messages.
                using (stream = new MemoryStream())
                using (var writer = new EmberWriter(stream))
                {
                    this.root.WriteChanges(writer, this);
                }

                await this.client.SendMessageAsync(this.emberDataMessage, stream.ToArray());
            }
        }

        /// <summary>Stops synchronizing changes to the object tree accessible through the <see cref="Root"/> property
        /// and raises the <see cref="ConnectionLost"/> event.</summary>
        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;
                this.root.HasChangesSet -= this.OnHasChangesSet;
                this.client.ConnectionLost -= this.receiveQueue.OnConnectionLost;
                this.client.EmberDataReceived -= this.receiveQueue.OnMessageReceived;
                this.hasChangesSetSource.TrySetCanceled();
                this.CancelAutoSendDelay();
                this.receiveQueue.OnConnectionLost(this, new ConnectionLostEventArgs(null));
            }
        }

        /// <summary>Returns the return value of
        /// <see cref="CreateAsync(S101Client, int)">CreateAsync(<paramref name="client"/>, 10000)</see>.</summary>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "There's no other way.")]
        public static Task<Consumer<TRoot>> CreateAsync(S101Client client)
        {
            return CreateAsync(client, 10000);
        }

        /// <summary>Returns the return value of <see cref="CreateAsync(S101Client, int, byte)">CreateAsync(<paramref name="client"/>,
        /// <paramref name="timeout"/>, 0x00)</see>.</summary>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "There's no other way.")]
        public static Task<Consumer<TRoot>> CreateAsync(S101Client client, int timeout)
        {
            return CreateAsync(client, timeout, 0x00);
        }

        /// <summary>Asynchronously uses <paramref name="client"/> to create a new <see cref="Consumer{T}"/> object.
        /// </summary>
        /// <param name="client">The <see cref="S101Client"/> to use.</param>
        /// <param name="timeout">The total amount of time, in milliseconds, this method will wait for the provider to
        /// send all requested elements. Specify -1 to wait indefinitely.</param>
        /// <param name="slot">The slot to communicate with. All outgoing <see cref="S101Message"/> objects will have
        /// their <see cref="S101Message.Slot"/> property set to this value. Incoming messages are ignored, if their
        /// <see cref="S101Message.Slot"/> property does not match this value.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout"/> is less than -1.</exception>
        /// <exception cref="Exception">An exception was thrown from one of the callbacks passed to the
        /// <see cref="S101Client"/> constructor, see <see cref="Exception.Message"/> for more information.</exception>
        /// <exception cref="ModelException">The model does either not match the data sent by the provider, or the
        /// provider has sent unexpected data.</exception>
        /// <exception cref="ObjectDisposedException"><see cref="S101Client.Dispose"/> has been called or the connection
        /// has been lost.</exception>
        /// <exception cref="OperationCanceledException"><see cref="S101Client.Dispose"/> has been called or the
        /// connection has been lost.</exception>
        /// <exception cref="TimeoutException">The provider did not send all requested elements within the specified
        /// <paramref name="timeout"/>.</exception>
        /// <remarks>
        /// <para>This method returns when initial values have been received for all non-optional
        /// <typeparamref name="TRoot"/> properties and recursively for all non-optional properties of
        /// <see cref="FieldNode{T}"/> subclass objects. Afterwards, all changes are continuously synchronized such that
        /// the state of the object tree accessible through the <see cref="Root"/> property mirrors the state of the
        /// tree held by the provider.</para>
        /// <para>All changes to the object tree are reported by raising the
        /// <see cref="INotifyPropertyChanged.PropertyChanged"/> event of the affected objects.</para>
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "There's no other way.")]
        public static async Task<Consumer<TRoot>> CreateAsync(S101Client client, int timeout, byte slot)
        {
            var result = new Consumer<TRoot>(client, timeout, slot);
            await result.QueryChildrenAsync();
            result.ReceiveLoop();
            result.AutoSendLoop();
            return result;
        }

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
        /// For the the last two cases <see cref="ConnectionLostEventArgs.Exception"/> is <c>null</c>.</para>
        /// </remarks>
        public event EventHandler<ConnectionLostEventArgs> ConnectionLost;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        int IInvocationCollection.Add(IInvocationResult invocationResult)
        {
            this.pendingInvocations.Add(++this.lastInvocationId, invocationResult);
            return this.lastInvocationId;
        }

        private Consumer(S101Client client, int timeout, byte slot)
        {
            this.client = client;
            this.queryChildrenTimeout = timeout;
            this.emberDataMessage = new S101Message(slot, EmberDataCommand);
            this.client.EmberDataReceived += this.receiveQueue.OnMessageReceived;
            this.client.ConnectionLost += this.receiveQueue.OnConnectionLost;
        }

        private void CancelAutoSendDelay()
        {
            if (!this.autoSendDelayCancellationSource.IsCancellationRequested)
            {
                this.autoSendDelayCancellationSource.Cancel();
            }
        }

        private void OnHasChangesSet(object sender, EventArgs e)
        {
            this.hasChangesSetSource.TrySetResult(true);
        }

        private async Task QueryChildrenAsync()
        {
            var queryChildrenTask = this.QueryChildrenAsyncCore();

            if ((await Task.WhenAny(queryChildrenTask, Task.Delay(this.queryChildrenTimeout))) != queryChildrenTask)
            {
                this.root.UpdateChildrenState(this.root.ChildrenState.Equals(ChildrenState.Complete));
                var firstIncompleteNode = this.root.GetFirstIncompleteChild();
                var message = firstIncompleteNode == null ?
                    "The provider failed to send all requested elements within the specified timeout." :
                    "The provider failed to send the children for the element with the path " +
                    firstIncompleteNode.GetPath() + ".";
                throw new TimeoutException(message);
            }

            await queryChildrenTask;
        }

        private async void ReceiveLoop()
        {
            Exception exception = null;

            try
            {
                while (true)
                {
                    await this.WaitForAndApplyChanges();
                    await this.QueryChildrenAsync();
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                this.OnConnectionLost(this, new ConnectionLostEventArgs(exception));
                this.Dispose();
            }
        }

        private async void AutoSendLoop()
        {
            this.autoSendDelayCancellationSource = new CancellationTokenSource();
            this.hasChangesSetSource = new TaskCompletionSource<bool>();
            this.root.HasChangesSet += this.OnHasChangesSet;

            try
            {
                while (true)
                {
                    await this.hasChangesSetSource.Task;
                    await this.DelayAutoSend();
                    this.hasChangesSetSource = new TaskCompletionSource<bool>();
                    await this.SendAsync();
                }

                // TODO: Exceptions should be propagated to client code via an event similar to
                // S101Client.ConnectionLost
            }
            catch (OperationCanceledException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            finally
            {
                this.CancelAutoSendDelay();
                this.autoSendDelayCancellationSource.Dispose();
                this.Dispose();
            }
        }

        private async Task QueryChildrenAsyncCore()
        {
            while (await this.SendChildrenQuery())
            {
                await this.WaitForAndApplyChanges();
            }
        }

        private async Task WaitForAndApplyChanges()
        {
            await this.receiveQueue.WaitForMessageAsync();

            while (this.receiveQueue.MessageCount > 0)
            {
                this.ApplyChange(this.receiveQueue.DequeueMessage());
            }
        }

        private void OnConnectionLost(object sender, ConnectionLostEventArgs e)
        {
            var handler = this.ConnectionLost;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        private async Task DelayAutoSend()
        {
            while (true)
            {
                try
                {
                    await Task.Delay(this.autoSendInterval, this.autoSendDelayCancellationSource.Token);
                    return;
                }
                catch (OperationCanceledException)
                {
                    if (this.disposed)
                    {
                        throw;
                    }
                    else
                    {
                        this.autoSendDelayCancellationSource = new CancellationTokenSource();
                    }
                }
            }
        }

        private async Task<bool> SendChildrenQuery()
        {
            var rootChildrenState = this.root.UpdateChildrenState(false);

            if (rootChildrenState.Equals(ChildrenState.None))
            {
                // There is no guarantee that the response we received is actually an answer to the previously sent
                // getDirectory request. It could just as well be a parameter value update. ChildrenState indicates
                // whether the former or the latter happened. If ChildrenState == ChildrenState.None, we have
                // received at least one new node. If ChildrenState != ChildrenState.None, no new node without
                // children has been received and consequently no new getDirectory request needs to be sent.
                // Of course, in the latter case the assumption is that the provider will at some point send an
                // answer to our previous getDirectory request. If it doesn't, the timeout will take care of things.
                MemoryStream stream;
                WriteChildrenQuery(this.root, out stream);
                await this.client.SendMessageAsync(this.emberDataMessage, stream.ToArray());
            }

            return !rootChildrenState.Equals(ChildrenState.Verified);
        }

        private void ApplyChange(MessageReceivedEventArgs args)
        {
            if (args.Message.Slot != this.emberDataMessage.Slot)
            {
                return;
            }

            var command = (EmberData)args.Message.Command;

            if (command.Dtd != EmberDataCommand.Dtd)
            {
                throw new ModelException(
                    string.Format(CultureInfo.InvariantCulture, "Unexpected DTD: {0:X2}.", command.Dtd));
            }

            var actualBytes = command.ApplicationBytes;
            var expectedBytes = EmberDataCommand.ApplicationBytes;

            if ((actualBytes.Count != expectedBytes.Count) ||
                (actualBytes.Zip(expectedBytes, (l, r) => l - r).Reverse().FirstOrDefault(b => b != 0) < 0))
            {
                throw new ModelException(string.Format(
                    CultureInfo.InvariantCulture,
                    "Encountered actual Glow DTD Version {0} while expecting >= {1}.",
                    GetVersion(actualBytes),
                    GetVersion(expectedBytes)));
            }

            var payload = args.GetPayload();

            // TODO: Reuse MemoryStream and EmberReader for all incoming messages.
            using (var stream = new MemoryStream(payload))
            using (var reader = new EmberReader(stream))
            {
                this.root.Read(reader, this.pendingInvocations);
            }
        }

        private static void WriteChildrenQuery(TRoot root, out MemoryStream stream)
        {
            // TODO: Reuse MemoryStream and EmberWriter for all outgoing messages.
            using (stream = new MemoryStream())
            using (var writer = new EmberWriter(stream))
            {
                root.WriteChildrenQuery(writer);
            }
        }

        private static string GetVersion(IReadOnlyCollection<byte> applicationBytes)
        {
            return string.Join(".", applicationBytes.Reverse().Select(b => b.ToString(CultureInfo.InvariantCulture)));
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private sealed class ReceiveQueue
        {
            private readonly Queue<MessageReceivedEventArgs> queue = new Queue<MessageReceivedEventArgs>();
            private readonly TaskCompletionSource<bool> connectionLost = new TaskCompletionSource<bool>();
            private TaskCompletionSource<bool> nonEmpty = new TaskCompletionSource<bool>();

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////

            internal void OnMessageReceived(object sender, MessageReceivedEventArgs e)
            {
                this.queue.Enqueue(e);

                if (!e.IsAnotherMessageAvailable)
                {
                    this.nonEmpty.TrySetResult(true);
                }
            }

            internal void OnConnectionLost(object sender, ConnectionLostEventArgs e)
            {
                if (e.Exception == null)
                {
                    this.connectionLost.TrySetCanceled();
                }
                else
                {
                    this.connectionLost.TrySetException(e.Exception);
                }
            }

            internal async Task WaitForMessageAsync()
            {
                await await Task.WhenAny(this.connectionLost.Task, this.nonEmpty.Task);
                this.nonEmpty = new TaskCompletionSource<bool>();
            }

            internal int MessageCount
            {
                get { return this.queue.Count; }
            }

            internal MessageReceivedEventArgs DequeueMessage()
            {
                return this.queue.Dequeue();
            }
        }
    }
}
