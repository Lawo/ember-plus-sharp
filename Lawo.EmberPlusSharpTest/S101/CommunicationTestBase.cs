////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.S101
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;

    using Ember;
    using IO;
    using UnitTesting;

    /// <summary>Provides methods that facilitate writing S101 communication unit tests.</summary>
    /// <threadsafety static="true" instance="false"/>
    public abstract class CommunicationTestBase : TestBase
    {
        /// <summary>Gets the <see cref="EmberData"/> instance for the current version.</summary>
        protected static EmberData EmberDataCommand { get; } = new EmberData(0x01, 0x0A, 0x02);

        /// <summary>Gets a <see cref="S101Message"/> message with an <see cref="EmberData"/> command for the current
        /// version.</summary>
        protected static S101Message EmberDataMessage { get; } = new S101Message(0x00, EmberDataCommand);

        /// <summary>Gets a <see cref="S101Message"/> message with an <see cref="KeepAliveRequest"/> command.</summary>
        protected static S101Message KeepAliveRequestMessage { get; } = new S101Message(0x00, new KeepAliveRequest());

        /// <summary>Gets a <see cref="S101Message"/> message with an <see cref="KeepAliveResponse"/> command.</summary>
        protected static S101Message KeepAliveResponseMessage { get; } = new S101Message(0x00, new KeepAliveResponse());

        /// <summary>Uses <see cref="S101Robot"/> to simulate a provider communicating with the <see cref="S101Client"/>
        /// object passed to <paramref name="testCallback"/>.</summary>
        /// <typeparam name="TResourceNamespace">The type whose namespace is used to scope <paramref name="logXmlName"/>
        /// in the manifest resources.</typeparam>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "There's no clean alternative.")]
        protected static Task TestWithRobot<TResourceNamespace>(
            Func<S101Client, Task> testCallback,
            IS101Logger consumerLogger,
            IS101Logger providerLogger,
            EmberTypeBag types,
            bool sendFirstMessage,
            string logXmlName,
            params object[] args)
        {
            var xml = string.Format(CultureInfo.InvariantCulture, GetContent<TResourceNamespace>(logXmlName), args);

            return TestNoExceptionsAsync(
                async (consumerClientTask, providerClient) =>
                {
                    using (var reader = new StringReader(xml))
                    using (var xmlReader = XmlReader.Create(reader))
                    {
                        var robotTask = S101Robot.RunAsync(providerClient, types, xmlReader, sendFirstMessage);

                        using (var consumerClient = await consumerClientTask)
                        {
                            var testTask = testCallback(consumerClient);

                            // The following lines ensure that exceptions thrown from either testTask or robotTask are
                            // immediately propagated up the call chain to fail the test. It also makes sure that
                            // both tasks complete when the first completed task did not throw an exception.
                            await await Task.WhenAny(testTask, robotTask);
                            await Task.WhenAll(testTask, robotTask);
                        }
                    }
                },
                () => ConnectAsync(-1, consumerLogger),
                () => WaitForConnectionAsync(providerLogger));
        }

        /// <summary>Asynchronously establishes a TCP connection between a consumer and provider
        /// <see cref="S101Client"/> object and passes them to <paramref name="testCallback"/>.</summary>
        /// <typeparam name="TConsumer">The type of the consumer to create.</typeparam>
        /// <remarks>If the <see cref="S101Client.ConnectionLost"/> event is fired with an exception, the test is
        /// automatically marked as failed.</remarks>
        protected static Task TestNoExceptionsAsync<TConsumer>(
            Func<TConsumer, S101Client, Task> testCallback,
            Func<Task<TConsumer>> createConsumer,
            Func<Task<S101Client>> createProvider)
            where TConsumer : IMonitoredConnection
        {
            return TestNoExceptionsAsync(
                (ct, p) => MonitorConnection(ct, c => testCallback(c, p)), createConsumer, createProvider);
        }

        /// <summary>Asynchronously establishes a TCP connection between a consumer and provider
        /// <see cref="S101Client"/> object and passes them to <paramref name="testCallback"/>.</summary>
        /// <typeparam name="TConsumer">The type of the consumer to create.</typeparam>
        /// <remarks>If the <see cref="S101Client.ConnectionLost"/> event is fired with an exception, the test is
        /// automatically marked as failed.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Test code.")]
        protected static Task TestNoExceptionsAsync<TConsumer>(
            Func<Task<TConsumer>, S101Client, Task> testCallback,
            Func<Task<TConsumer>> createConsumer,
            Func<Task<S101Client>> createProvider)
            where TConsumer : IMonitoredConnection
        {
            var providerTask = createProvider();
            var consumerTask = createConsumer();
            return MonitorConnection(providerTask, p => testCallback(consumerTask, p));
        }

        /// <summary>Establishes a new TCP connection with the local host on port 8099.</summary>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The TcpClient object is stored inside the returned S101Client object.")]
        protected static async Task<S101Client> ConnectAsync(int timeout, IS101Logger logger)
        {
            var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync("localhost", 8099);
            var stream = tcpClient.GetStream();
            return new S101Client(tcpClient, stream.ReadAsync, stream.WriteAsync, logger, timeout, 8192);
        }

        /// <summary>Asynchronously waits for a S101 connection to port 8099.</summary>
        protected static async Task<S101Client> WaitForConnectionAsync(IS101Logger logger)
        {
            var tcpClient = await WaitForConnectionAsync();
            var stream = tcpClient.GetStream();
            return new S101Client(tcpClient, stream.ReadAsync, stream.WriteAsync, logger, -1, 8192);
        }

        /// <summary>Asynchronously waits for a TCP connection to port 8099.</summary>
        protected static Task<TcpClient> WaitForConnectionAsync() => WaitForConnectionAsync(8099);

        /// <summary>Asynchronously waits for a TCP connection to port 8099.</summary>
        protected static async Task<TcpClient> WaitForConnectionAsync(int port)
        {
            var listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            var tcpClient = await listener.AcceptTcpClientAsync();
            listener.Stop();
            return tcpClient;
        }

        /// <summary>Handles the <see cref="S101Client.ConnectionLost"/> event.</summary>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Test code.")]
        protected static void OnConnectionLost(TaskCompletionSource<bool> source, ConnectionLostEventArgs args)
        {
            if (args.Exception == null)
            {
                source.SetResult(true);
            }
            else
            {
                source.SetException(args.Exception);
            }
        }

        /// <summary>Gets a <see cref="S101Client"/> object, which receives the message contained in
        /// <paramref name="messageStream"/> as soon as a message has been sent by calling
        /// <see cref="S101Client.SendMessageAsync(S101Message)"/>.</summary>
        protected static S101Client GetFakeClient(Stream messageStream, IS101Logger logger)
        {
            var disposable = new CompleteOnDispose();
            var requestReceived = new TaskCompletionSource<bool>();

            ReadAsyncCallback read =
                async (b, o, c, t) =>
                {
                    await requestReceived.Task;
                    return await Read(messageStream, b, o, c, t, disposable.Task);
                };

            WriteAsyncCallback write =
                (b, o, c, t) =>
                {
                    requestReceived.SetResult(true);
                    return Task.FromResult(false);
                };

            return new S101Client(disposable, read, write, logger, -1, 8192);
        }

        /// <summary>Gets a <see cref="MemoryStream"/> with a message containing an <see cref="EmberData"/>
        /// command and the <paramref name="payload"/>.</summary>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Must be disposed by the caller.")]
        protected static async Task<MemoryStream> GetS101MessageStreamAsync(byte[] payload)
        {
            var stream = new MemoryStream();
            var writer = new S101Writer(stream.WriteAsync);

            using (var messageStream = await writer.WriteMessageAsync(EmberDataMessage, CancellationToken.None))
            {
                if (payload != null)
                {
                    await messageStream.WriteAsync(payload, 0, payload.Length);
                }

                await messageStream.DisposeAsync(CancellationToken.None);
            }

            await writer.DisposeAsync(CancellationToken.None);

            stream.Position = 0;
            return stream;
        }

        /// <summary>Monitors a given connection for connection loss.</summary>
        /// <typeparam name="TConnection">The type of the connection.</typeparam>
        protected static async Task MonitorConnection<TConnection>(
            Task<TConnection> connectionTask,
            Func<TConnection, Task> callback)
            where TConnection : IMonitoredConnection
        {
            Task waitForConnectionLost;

            using (var connection = await connectionTask)
            {
                waitForConnectionLost = WaitForConnectionLostAsync(connection);
                var callbackTask = callback(connection);

                if (await Task.WhenAny(callbackTask, waitForConnectionLost) == waitForConnectionLost)
                {
                    // This ensures that an exception is propagated up the call chain. If the connection was closed
                    // regularly, we also need to wait for the callbackTask to complete.
                    await waitForConnectionLost;
                }

                await callbackTask;
            }

            await waitForConnectionLost;
        }

        /// <summary>Reads and returns the contents of the resource text file identified by <paramref name="fileName"/>.
        /// </summary>
        /// <typeparam name="TResourceNamespace">The type whose namespace is used to scope <paramref name="fileName"/>
        /// in the manifest resources.</typeparam>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "There's no clean alternative.")]
        protected static string GetContent<TResourceNamespace>(string fileName)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                typeof(TResourceNamespace), fileName))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        /// <summary>Initializes a new instance of the <see cref="CommunicationTestBase"/> class.</summary>
        protected CommunicationTestBase()
        {
        }

        /// <summary>Gets a random byte that is guaranteed to not be equal to any of the elements in
        /// <paramref name="exceptions"/>.</summary>
        protected byte GetRandomByteExcept(params byte[] exceptions)
        {
            byte result;

            while (exceptions.Contains(result = (byte)Random.Shared.Next(byte.MinValue, byte.MaxValue + 1)))
            {
            }

            return result;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static Task<int> Read(
            Stream stream,
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken,
            Task<int> waitUntilCancelled)
        {
            return (stream.Position < stream.Length) ?
                stream.ReadAsync(buffer, offset, count, cancellationToken) : waitUntilCancelled;
        }

        private static Task WaitForConnectionLostAsync(IMonitoredConnection connection)
        {
            var connectionLost = new TaskCompletionSource<bool>();
            EventHandler<ConnectionLostEventArgs> handler = null;
            handler =
                (s, e) =>
                {
                    if (e.Exception == null)
                    {
                        connectionLost.SetResult(true);
                    }
                    else
                    {
                        connectionLost.SetException(e.Exception);
                    }

                    connection.ConnectionLost -= handler;
                };
            connection.ConnectionLost += handler;
            return connectionLost.Task;
        }
    }
}
