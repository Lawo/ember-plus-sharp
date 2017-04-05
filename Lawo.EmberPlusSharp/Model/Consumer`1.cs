////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Ember;
    using IO;
    using S101;

    using static System.Globalization.CultureInfo;

    /// <summary>Implements an Ember+ consumer that communicates with an Ember+ provider as specified in the
    /// <i>"Ember+ Specification"</i><cite>Ember+ Specification</cite>.</summary>
    /// <typeparam name="TRoot">The type of the root of the object tree that will mirror the state of the tree published
    /// by the provider.</typeparam>
    /// <threadsafety static="true" instance="false"/>
    public sealed class Consumer<TRoot> : IMonitoredConnection
        where TRoot : Root<TRoot>
    {
        /// <summary>Returns the return value of
        /// <see cref="CreateAsync(S101Client, int)">CreateAsync(<paramref name="client"/>, 10000)</see>.</summary>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "There's no other way.")]
        public static Task<Consumer<TRoot>> CreateAsync(S101Client client) => CreateAsync(client, 10000);

        /// <summary>Returns the return value of <see cref="CreateAsync(S101Client, int, byte)">CreateAsync(<paramref name="client"/>,
        /// <paramref name="timeout"/>, 0x00)</see>.</summary>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "There's no other way.")]
        public static Task<Consumer<TRoot>> CreateAsync(S101Client client, int timeout) =>
            CreateAsync(client, timeout, (byte)0x00);

        /// <summary>Returns the return value of <see cref="CreateAsync(S101Client, int, ChildrenRetrievalPolicy, byte)">
        /// CreateAsync(<paramref name="client"/>, <paramref name="timeout"/>, <paramref name="childrenRetrievalPolicy"/>,
        /// 0x00)</see>.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "There's no other way.")]
        public static Task<Consumer<TRoot>> CreateAsync(
            S101Client client, int timeout, ChildrenRetrievalPolicy childrenRetrievalPolicy)
        {
            return CreateAsync(client, timeout, childrenRetrievalPolicy, 0x00);
        }

        /// <summary>Returns the return value of <see cref="CreateAsync(S101Client, int, ChildrenRetrievalPolicy, byte)">
        /// CreateAsync(<paramref name="client"/>, <paramref name="timeout"/>,
        /// <see cref="ChildrenRetrievalPolicy.All">ChildrenRetrievalPolicy.All</see>, <paramref name="slot"/>)</see>.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "There's no other way.")]
        public static Task<Consumer<TRoot>> CreateAsync(S101Client client, int timeout, byte slot) =>
            CreateAsync(client, timeout, ChildrenRetrievalPolicy.All, slot);

        /// <summary>Asynchronously uses <paramref name="client"/> to create a new <see cref="Consumer{T}"/> object.
        /// </summary>
        /// <param name="client">The <see cref="S101Client"/> to use.</param>
        /// <param name="timeout">The total amount of time, in milliseconds, this method will wait for the provider to
        /// send all requested elements. Specify -1 to wait indefinitely.</param>
        /// <param name="childrenRetrievalPolicy">The policy that defines whether direct and indirect children are
        /// retrieved from the provider before this method returns.</param>
        /// <param name="slot">The slot to communicate with. All outgoing <see cref="S101Message"/> objects will have
        /// their <see cref="S101Message.Slot"/> property set to this value. Incoming messages are ignored, if their
        /// <see cref="S101Message.Slot"/> property does not match this value.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <list type="bullet">
        /// <item><paramref name="timeout"/> is less than -1, and/or</item>
        /// <item><paramref name="childrenRetrievalPolicy"/> is either less than
        /// <see cref="ChildrenRetrievalPolicy.None"/> or greater than <see cref="ChildrenRetrievalPolicy.All"/>.
        /// </item>
        /// </list></exception>
        /// <exception cref="ArgumentNullException"><paramref name="client"/> equals <c>null</c>.</exception>
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
        /// <para>Sets the <see cref="INode.ChildrenRetrievalPolicy"/> property of the <see cref="Root"/> object to the
        /// value passed for <paramref name="childrenRetrievalPolicy"/> and then retrieves a partial or full copy of the
        /// provider tree before returning the <see cref="Consumer{TRoot}"/> object. Exactly what elements are initially
        /// retrieved from the provider depends on the type of <typeparamref name="TRoot"/> and the value of
        /// <paramref name="childrenRetrievalPolicy"/>.</para>
        /// <para>Afterwards, all changes are continuously synchronized such that the state of the object tree
        /// accessible through the <see cref="Root"/> property mirrors the state of the tree held by the provider.
        /// </para>
        /// <para>All changes to the object tree are reported by raising the
        /// <see cref="INotifyPropertyChanged.PropertyChanged"/> event of the affected objects.</para>
        /// </remarks>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1625:Element documentation must not be copied and pasted", Justification = "Intended, both exceptions can be thrown under the same circumstances.")]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "There's no other way.")]
        public static async Task<Consumer<TRoot>> CreateAsync(
            S101Client client, int timeout, ChildrenRetrievalPolicy childrenRetrievalPolicy, byte slot)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if ((childrenRetrievalPolicy < ChildrenRetrievalPolicy.None) ||
                (childrenRetrievalPolicy > ChildrenRetrievalPolicy.All))
            {
                throw new ArgumentOutOfRangeException(nameof(childrenRetrievalPolicy));
            }

            var result = new Consumer<TRoot>(client, timeout, childrenRetrievalPolicy, slot);
            await result.RetrieveChildrenAsync();
            result.SendReceiveLoop();
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
        /// For the last two cases <see cref="ConnectionLostEventArgs.Exception"/> is <c>null</c>.</para>
        /// </remarks>
        public event EventHandler<ConnectionLostEventArgs> ConnectionLost;

        /// <summary>Gets the root of the object tree that mirrors the state of the tree published by the provider.
        /// </summary>
        public TRoot Root => this.root;

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
                    throw new ArgumentOutOfRangeException(nameof(value), "Must be >= -1.");
                }

                this.autoSendInterval = value;
                this.CancelAutoSendDelay();
            }
        }

        /// <summary>Asynchronously sends the locally applied changes and invocations to the provider.</summary>
        /// <exception cref="Exception">An exception was thrown from one of the callbacks passed to the
        /// <see cref="S101Client"/> constructor, see <see cref="Exception.Message"/> for more information.</exception>
        /// <exception cref="InvalidOperationException">This method was called from a thread other than the one that
        /// executed <see cref="CreateAsync(S101Client, int, ChildrenRetrievalPolicy, byte)"/>.</exception>
        /// <exception cref="ObjectDisposedException"><see cref="Dispose"/> has been called or the connection has been
        /// lost.</exception>
        /// <exception cref="OperationCanceledException"><see cref="Dispose"/> has been called or the connection has
        /// been lost.</exception>
        /// <remarks>Also retrieves the children of any objects implementing <see cref="INode"/> that have had their
        /// <see cref="INode.ChildrenRetrievalPolicy"/> property set to a value other than
        /// <see cref="ChildrenRetrievalPolicy.None">ChildrenRetrievalPolicy.None</see>.</remarks>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1625:Element documentation must not be copied and pasted", Justification = "Intended, both exceptions can be thrown under the same circumstances.")]
        public Task SendAsync()
        {
            var source = new TaskCompletionSource<bool>();

            this.taskQueue.Enqueue(
                async () =>
                {
                    await this.SendCoreAsync();
                    source.SetResult(true);
                });

            return source.Task;
        }

        /// <summary>Stops synchronizing changes to the object tree accessible through the <see cref="Root"/> property
        /// and raises the <see cref="ConnectionLost"/> event.</summary>
        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;
                this.root.SendRequired -= this.OnSendRequired;
                this.client.ConnectionLost -= this.OnConnectionLost;
                this.client.EmberDataReceived -= this.OnMessageReceived;
                this.client.ConnectionLost -= this.receiveQueue.OnConnectionLost;
                this.client.EmberDataReceived -= this.receiveQueue.OnMessageReceived;
                this.CancelAutoSendDelay();
                this.receiveQueue.OnConnectionLost(this, new ConnectionLostEventArgs(null));
                this.OnConnectionLost(this, new ConnectionLostEventArgs(null));
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static readonly EmberData EmberDataCommand = new EmberData(0x01, 0x0A, 0x02);

        private static string GetVersion(IReadOnlyCollection<byte> applicationBytes) =>
            string.Join(".", applicationBytes.Reverse().Select(b => b.ToString(InvariantCulture)));

        private readonly TaskQueue taskQueue = new TaskQueue();
        private readonly ReceiveQueue receiveQueue = new ReceiveQueue();
        private readonly InvocationCollection pendingInvocations = new InvocationCollection();
        private readonly StreamedParameterCollection streamedParameters = new StreamedParameterCollection();
        private readonly TRoot root;
        private readonly S101Client client;
        private readonly int childrenRetrievalTimeout;
        private readonly S101Message emberDataMessage;
        private int autoSendInterval = 100;
        private CancellationTokenSource autoSendDelayCancellationSource;
        private bool disposed;

        private Consumer(S101Client client, int timeout, ChildrenRetrievalPolicy childrenRetrievalPolicy, byte slot)
        {
            this.root = Root<TRoot>.Construct(new Context(null, 0, string.Empty, childrenRetrievalPolicy));
            this.client = client;
            this.childrenRetrievalTimeout = timeout;
            this.emberDataMessage = new S101Message(slot, EmberDataCommand);
            this.client.EmberDataReceived += this.receiveQueue.OnMessageReceived;
            this.client.ConnectionLost += this.receiveQueue.OnConnectionLost;
        }

        private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (!e.IsAnotherMessageAvailable)
            {
                this.taskQueue.Enqueue(
                    async () =>
                    {
                        // We must not wait for changes here, because that would lead to a deadlock when client code
                        // calls SendAsync. Instead we just apply whatever is left in the queue and retrieve children
                        // (if there are any). If the queue is already empty (because client code has called SendAsync)
                        // and everything is complete, the two statements below don't do anything.
                        this.ApplyProviderChanges();
                        await this.RetrieveChildrenAsync();
                    });
            }
        }

        private void OnConnectionLost(object sender, ConnectionLostEventArgs e) =>
            this.taskQueue.Enqueue(() => this.WaitForProviderChangesAsync());

        private async Task SendCoreAsync()
        {
            if (this.root.HasChanges)
            {
                MemoryStream stream;

                // TODO: Reuse MemoryStream and EmberWriter for all outgoing messages.
                using (stream = new MemoryStream())
                using (var writer = new EmberWriter(stream))
                {
                    this.root.WriteChanges(writer, this.pendingInvocations);
                }

                await this.client.SendMessageAsync(this.emberDataMessage, stream.ToArray());
            }

            await this.RetrieveChildrenAsync();
        }

        private void CancelAutoSendDelay()
        {
            if (!this.autoSendDelayCancellationSource.IsCancellationRequested)
            {
                this.autoSendDelayCancellationSource.Cancel();
            }
        }

        private async void OnSendRequired(object sender, EventArgs e)
        {
            if ((this.autoSendInterval != Timeout.Infinite) && (await this.DelayAutoSend()))
            {
                this.taskQueue.Enqueue(this.SendCoreAsync);
            }
        }

        private async Task RetrieveChildrenAsync()
        {
            var retrieveChildrenTask = this.RetrieveChildrenCoreAsync();

            if ((await Task.WhenAny(retrieveChildrenTask, Task.Delay(this.childrenRetrievalTimeout))) !=
                retrieveChildrenTask)
            {
                this.root.UpdateRetrievalState(this.root.RetrievalState.Equals(RetrievalState.Complete));
                var firstIncompleteNode = this.root.GetFirstIncompleteChild();
                var message = firstIncompleteNode == null ?
                    "The provider failed to send all requested elements within the specified timeout." :
                    "The provider failed to send the children for the element with the path " +
                    firstIncompleteNode.GetPath() + ".";
                throw new TimeoutException(message);
            }

            await retrieveChildrenTask;
        }

        private async void SendReceiveLoop()
        {
            Exception exception = null;
            this.autoSendDelayCancellationSource = new CancellationTokenSource();
            this.client.EmberDataReceived += this.OnMessageReceived;
            this.client.ConnectionLost += this.OnConnectionLost;
            this.root.SendRequired += this.OnSendRequired;

            try
            {
                while (true)
                {
                    await this.taskQueue.ExecuteAsync();
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                this.ConnectionLost?.Invoke(this, new ConnectionLostEventArgs(exception));
                this.CancelAutoSendDelay();
                this.autoSendDelayCancellationSource.Dispose();
                this.Dispose();
            }
        }

        private async Task RetrieveChildrenCoreAsync()
        {
            while (await this.SendRequestAsync())
            {
                await this.WaitForProviderChangesAsync();
                this.ApplyProviderChanges();
            }
        }

        private void ApplyProviderChanges()
        {
            while (this.receiveQueue.MessageCount > 0)
            {
                this.ApplyChange(this.receiveQueue.DequeueMessage());
            }
        }

        private Task WaitForProviderChangesAsync() => this.receiveQueue.WaitForMessageAsync();

        private async Task<bool> DelayAutoSend()
        {
            while (true)
            {
                try
                {
                    await Task.Delay(this.autoSendInterval, this.autoSendDelayCancellationSource.Token);
                    return true;
                }
                catch (OperationCanceledException)
                {
                    if (this.disposed)
                    {
                        return false;
                    }
                    else
                    {
                        this.autoSendDelayCancellationSource = new CancellationTokenSource();
                    }
                }
            }
        }

        private async Task<bool> SendRequestAsync()
        {
            var rootRetrievalState = this.root.UpdateRetrievalState(false);

            if (rootRetrievalState.Equals(RetrievalState.None))
            {
                // There is no guarantee that the response we received is actually an answer to the previously sent
                // getDirectory request. It could just as well be a parameter value update. RetrievalState indicates
                // whether the former or the latter happened. If RetrievalState == RetrievalState.None, we have
                // received at least one new node. If RetrievalState != RetrievalState.None, no new node without
                // children has been received and consequently no new getDirectory request needs to be sent.
                // Of course, in the latter case the assumption is that the provider will at some point send an
                // answer to our previous getDirectory request. If it doesn't, the timeout will take care of things.
                MemoryStream stream;

                if (!this.WriteRequest(out stream))
                {
                    // If no answer is expected from the provider due to the request, we need to update the request
                    // state again to see whether there's still something missing.
                    rootRetrievalState = this.root.UpdateRetrievalState(false);
                }

                await this.client.SendMessageAsync(this.emberDataMessage, stream.ToArray());
            }

            return !rootRetrievalState.Equals(RetrievalState.Verified);
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
                throw new ModelException(string.Format(InvariantCulture, "Unexpected DTD: {0:X2}.", command.Dtd));
            }

            var actualBytes = command.ApplicationBytes;
            var expectedBytes = EmberDataCommand.ApplicationBytes;

            if ((actualBytes.Count != expectedBytes.Count) ||
                (actualBytes.Zip(expectedBytes, (l, r) => l - r).Reverse().FirstOrDefault(b => b != 0) < 0))
            {
                throw new ModelException(string.Format(
                    InvariantCulture,
                    "Encountered actual Glow DTD Version {0} while expecting >= {1}.",
                    GetVersion(actualBytes),
                    GetVersion(expectedBytes)));
            }

            var payload = args.GetPayload();

            // TODO: Reuse MemoryStream and EmberReader for all incoming messages.
            using (var stream = new MemoryStream(payload))
            using (var reader = new EmberReader(stream))
            {
                this.root.Read(reader, this.pendingInvocations, this.streamedParameters);
            }
        }

        private bool WriteRequest(out MemoryStream stream)
        {
            // TODO: Reuse MemoryStream and EmberWriter for all outgoing messages.
            using (stream = new MemoryStream())
            using (var writer = new EmberWriter(stream))
            {
                return this.root.WriteRequest(writer, this.streamedParameters);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private sealed class TaskQueue
        {
            internal void Enqueue(Func<Task> task)
            {
                this.queue.Enqueue(task);

                if (this.queue.Count == 1)
                {
                    this.nonEmpty.SetResult(true);
                }
            }

            internal async Task ExecuteAsync()
            {
                await this.nonEmpty.Task;
                var tasks = new Func<Task>[this.queue.Count];
                this.queue.CopyTo(tasks, 0);
                this.queue.Clear();
                this.nonEmpty = new TaskCompletionSource<bool>();

                foreach (var task in tasks)
                {
                    await task();
                }
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////

            private readonly Queue<Func<Task>> queue = new Queue<Func<Task>>();
            private TaskCompletionSource<bool> nonEmpty = new TaskCompletionSource<bool>();
        }

        private sealed class ReceiveQueue
        {
            internal int MessageCount => this.queue.Count;

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

            internal MessageReceivedEventArgs DequeueMessage() => this.queue.Dequeue();

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////

            private readonly Queue<MessageReceivedEventArgs> queue = new Queue<MessageReceivedEventArgs>();
            private readonly TaskCompletionSource<bool> connectionLost = new TaskCompletionSource<bool>();
            private TaskCompletionSource<bool> nonEmpty = new TaskCompletionSource<bool>();
        }
    }
}
