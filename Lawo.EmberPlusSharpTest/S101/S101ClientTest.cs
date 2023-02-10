////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.S101
{
    using System;
    using System.IO;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

    using Ember;
    using EmberDataPayloads;
    using Glow;
    using IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Threading.Tasks;

    /// <summary>Tests <see cref="S101Client"/>.</summary>
    [TestClass]
    public class S101ClientTest : CommunicationTestBase
    {
        /// <summary>Tests automatic keep alive with a provider that responds.</summary>
        [TestMethod]
        public void KeepAliveMainTest()
        {
            AsyncPump.Run(
                async () =>
                {
                    var providerClientTask = WaitForConnectionAsync();
                    int timeout = Random.Shared.Next(1000, 2000);
                    Console.WriteLine("Timeout: {0}", timeout);

                    using (var consumer = await ConnectAsync(timeout, null))
                    {
                        var slot = (byte)Random.Shared.Next(byte.MaxValue + 1);
                        consumer.KeepAliveRequestSlot = slot;
                        Assert.AreEqual(slot, consumer.KeepAliveRequestSlot);

                        var connectionLost = new TaskCompletionSource<bool>();
                        consumer.ConnectionLost += (s, e) => OnConnectionLost(connectionLost, e);
                        var providerClient = await providerClientTask;
                        var stream = providerClient.GetStream();

                        using (new S101Client(
                            providerClient,
                            stream.ReadAsync,
                            stream.WriteAsync,
                            new S101Logger(GlowTypes.Instance, Console.Out),
                            Timeout.Infinite,
                            8192))
                        {
                            await Task.Delay(timeout + (timeout / 4));
                        }

                        await connectionLost.Task;
                    }
                });
        }

        /// <summary>Tests automatic keep alive with a provider that does not respond.</summary>
        [TestMethod]
        public void KeepAliveExceptionTest()
        {
            AsyncPump.Run(
                async () =>
                {
                    var providerTask = WaitForConnectionAsync();
                    int timeout = Random.Shared.Next(4000, 8000);
                    Console.WriteLine("Timeout: {0}", timeout);

                    using (var consumer = new TcpClient("localhost", 8099))
                    using (var stream = consumer.GetStream())
                    using (var logger = new S101Logger(GlowTypes.Instance, Console.Out))
                    using (var consumerClient =
                        new S101Client(consumer, stream.ReadAsync, stream.WriteAsync, logger, timeout, 8192))
                    {
                        (await providerTask).Ignore();
                        consumerClient.KeepAliveRequestSlot = (byte)Random.Shared.Next(byte.MaxValue + 1);
                        var source = new TaskCompletionSource<bool>();
                        consumerClient.ConnectionLost += (s, e) => OnConnectionLost(source, e);
                        var task = await Task.WhenAny(source.Task, Task.Delay(timeout + (timeout / 4)));
                        await Assert.ThrowsExceptionAsync<S101Exception>(() => task);
                    }
                });
        }

        /// <summary>Tests sending/receiving messages with <see cref="EmberData"/> commands.</summary>
        [TestMethod]
        public void EmberDataTest()
        {
            AsyncPump.Run(() => TestNoExceptionsAsync(
                async (consumer, provider) =>
                {
                    var slot = (byte)Random.Shared.Next(byte.MaxValue + 1);
                    var data = new byte[Random.Shared.Next(512, 16384)];
                    Random.Shared.NextBytes(data);

                    var emberDataReceived = new TaskCompletionSource<bool>();
                    EventHandler<MessageReceivedEventArgs> emberDataHandler =
                        (s, e) =>
                        {
                            Assert.AreEqual(slot, e.Message.Slot);
                            Assert.IsInstanceOfType(e.Message.Command, typeof(EmberData));
                            CollectionAssert.AreEqual(data, e.GetPayload());
                            emberDataReceived.SetResult(true);
                        };

                    var outOfFrameByte = this.GetRandomByteExcept(0xFE);
                    var outOfFrameByteReceived = new TaskCompletionSource<bool>();
                    EventHandler<OutOfFrameByteReceivedEventArgs> outOfFrameByteHandler =
                        (s, e) =>
                        {
                            Assert.AreEqual(outOfFrameByte, e.Value);
                            outOfFrameByteReceived.SetResult(true);
                        };

                    provider.EmberDataReceived += emberDataHandler;
                    provider.OutOfFrameByteReceived += outOfFrameByteHandler;
                    await consumer.SendMessageAsync(new S101Message(slot, EmberDataCommand), data);
                    await emberDataReceived.Task;
                    await consumer.SendOutOfFrameByteAsync(outOfFrameByte);
                    await outOfFrameByteReceived.Task;
                    provider.OutOfFrameByteReceived -= outOfFrameByteHandler;
                    provider.EmberDataReceived -= emberDataHandler;
                },
                () => ConnectAsync(-1, null),
                () => WaitForConnectionAsync(null)));
        }

        /// <summary>Tests what happens when the connection is lost.</summary>
        [TestMethod]
        public void ConnectionLostTest()
        {
            AsyncPump.Run(
                async () =>
                {
                    var readResult = new TaskCompletionSource<int>();
                    using (var client = new S101Client(
                        new MemoryStream(),
                        (b, o, c, t) => readResult.Task,
                        (b, o, c, t) => Task.FromResult(false),
                        new S101Logger(GlowTypes.Instance, Console.Out)))
                    {
                        var exception = new IOException();
                        var connectionLost = new TaskCompletionSource<bool>();

                        client.ConnectionLost +=
                            (s, e) =>
                            {
                                Assert.AreEqual(exception, e.Exception);
                                connectionLost.SetResult(true);
                            };

                        readResult.SetException(exception);
                        await connectionLost.Task;
                        await Assert.ThrowsExceptionAsync<ObjectDisposedException>(
                            () => client.SendMessageAsync(new S101Message(0x00, new KeepAliveRequest())));
                    }
                });
        }

        /// <summary>Tests <see cref="S101Client"/> exceptions.</summary>
        [TestMethod]
        public void ExceptionTest()
        {
            using (var dummy = new MemoryStream())
            {
                ReadAsyncCallback fakeRead = (b, o, c, t) => Task.FromResult(0);
                WriteAsyncCallback fakeWrite = (b, o, c, t) => Task.FromResult(false);
                Assert.ThrowsException<NotSupportedException>(() => new S101Client(dummy, fakeRead, fakeWrite).Dispose());

                AsyncPump.Run(
                    async () =>
                    {
                        using (var connection = new CompleteOnDispose())
                        using (var client = new S101Client(connection, (b, o, c, t) => connection.Task, fakeWrite))
                        {
                            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                                () => Task.Run(() => client.SendMessageAsync(new S101Message(0x00, new KeepAliveRequest()))));
                        }

                        Assert.ThrowsException<ArgumentNullException>(
                            () => new S101Client(null, fakeRead, fakeWrite).Dispose());
                        Assert.ThrowsException<ArgumentNullException>(
                            () => new S101Client(dummy, null, fakeWrite).Dispose());
                        Assert.ThrowsException<ArgumentNullException>(
                            () => new S101Client(dummy, fakeRead, null).Dispose());

                        Assert.ThrowsException<ArgumentOutOfRangeException>(
                            () => new S101Client(dummy, fakeRead, fakeWrite, null, 3000, 0).Dispose());
                        Assert.ThrowsException<ArgumentOutOfRangeException>(
                            () => new S101Client(dummy, fakeRead, fakeWrite, null, -2, 1).Dispose());

                        using (var connection = new CompleteOnDispose())
                        using (var client = new S101Client(
                            connection, (b, o, c, t) => connection.Task, fakeWrite, null, 3000, 1))
                        {
                            await Assert.ThrowsExceptionAsync<ArgumentNullException>(
                                () => client.SendMessageAsync(null));
                            await Assert.ThrowsExceptionAsync<ArgumentException>(() => client.SendMessageAsync(EmberDataMessage));
                            await Assert.ThrowsExceptionAsync<ArgumentException>(() => client.SendOutOfFrameByteAsync(0xFE));

                            client.Dispose();
                            await Assert.ThrowsExceptionAsync<ObjectDisposedException>(
                                () => client.SendMessageAsync(new S101Message(0x00, new KeepAliveRequest())));
                        }
                    });
            }
        }

        /// <summary>Tests <see cref="EmberData"/> version handling.</summary>
        [TestMethod]
        public void VersionTest()
        {
            AsyncPump.Run(() => TestWithRobot<S101Payloads>(
                client =>
                {
                    client.EmberDataReceived += (s, e) =>
                    {
                        Console.WriteLine(e.Message.Command.ToString());
                    };

                    return Task.FromResult(false);
                },
                null,
                null,
                new EmberTypeBag(),
                true,
                "VersionLog.xml"));
        }
    }
}
