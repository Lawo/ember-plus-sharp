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
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Threading.Tasks;

    /// <summary>Tests <see cref="MessageEncodingStream"/>.</summary>
    [TestClass]
    public class S101WriterTest : CommunicationTestBase
    {
        /// <summary>Tests <see cref="S101Writer.WriteMessageAsync"/>.</summary>
        [TestMethod]
        public void WriteTest()
        {
            AsyncPump.Run(
                async () =>
                {
                    CollectionAssert.AreEqual(
                        new byte[] { 0xFE, 0x00, 0x0E, 0x01, 0x01, 0x94, 0xE4, 0xFF },
                        await Encode(new S101Message(0x00, new KeepAliveRequest())));
                    CollectionAssert.AreEqual(
                        new byte[] { 0xFE, 0x00, 0x0E, 0x02, 0x01, 0xFD, 0xDC, 0xCE, 0xFF },
                        await Encode(new S101Message(0x00, new KeepAliveResponse())));
                    CollectionAssert.AreEqual(
                        new byte[]
                        {
                            0xFE, 0x00, 0x0E, 0x00, 0x01, 0x80, 0x01, 0x02, 0x0a, 0x02, 0xF5, 0x78, 0xFF,
                            0xFE, 0x00, 0x0E, 0x00, 0x01, 0x60, 0x01, 0x02, 0x0a, 0x02, 0x13, 0x53, 0xFF
                        },
                        await Encode(EmberDataMessage, new byte[] { }));
                });
        }

        /// <summary>Tests <see cref="S101Writer.WriteMessageAsync"/>.</summary>
        [TestMethod]
        public void OutOfFrameByteTest()
        {
            AsyncPump.Run(
                async () =>
                {
                    var first = this.GetRandomByteExcept(0xFE);
                    var second = this.GetRandomByteExcept(0xFE);

                    var prefix = new[] { this.GetRandomByteExcept() };
                    var third = this.GetRandomByteExcept(0xFE);
                    var postfix = new[] { this.GetRandomByteExcept() };

                    using (var asyncStream = new MemoryStream())
                    {
                        var writer = new S101Writer(asyncStream.WriteAsync);

                        try
                        {
                            await writer.WriteOutOfFrameByteAsync(first, CancellationToken.None);
                            await writer.WriteMessageAsync(KeepAliveRequestMessage, CancellationToken.None);
                            await writer.WriteOutOfFrameByteAsync(second, CancellationToken.None);

                            using (var encodingStream = await writer.WriteMessageAsync(EmberDataMessage, CancellationToken.None))
                            {
                                await encodingStream.WriteAsync(prefix, 0, prefix.Length, CancellationToken.None);
                                await writer.WriteOutOfFrameByteAsync(third, CancellationToken.None);
                                await encodingStream.WriteAsync(postfix, 0, postfix.Length, CancellationToken.None);
                                await encodingStream.DisposeAsync(CancellationToken.None);
                            }
                        }
                        finally
                        {
                            await writer.DisposeAsync(CancellationToken.None);
                        }

                        asyncStream.Position = 0;
                        var reader = new S101Reader(asyncStream.ReadAsync);
                        var firstTask = WaitForOutOfFrameByte(reader);
                        Assert.IsTrue(await reader.ReadAsync(CancellationToken.None));
                        Assert.AreEqual(0x00, reader.Message.Slot);
                        Assert.IsInstanceOfType(reader.Message.Command, typeof(KeepAliveRequest));
                        Assert.AreEqual(first, await firstTask);
                        var secondTask = WaitForOutOfFrameByte(reader);
                        Assert.IsTrue(await reader.ReadAsync(CancellationToken.None));
                        Assert.AreEqual(0x00, reader.Message.Slot);
                        Assert.IsInstanceOfType(reader.Message.Command, typeof(EmberData));
                        Assert.AreEqual(second, await secondTask);
                        var thirdTask = WaitForOutOfFrameByte(reader);

                        using (var payloadStream = new MemoryStream())
                        {
                            await reader.Payload.CopyToAsync(payloadStream);
                            var payload = payloadStream.ToArray();
                            Assert.AreEqual(2, payload.Length);
                            Assert.AreEqual(prefix.Single(), payload[0]);
                            Assert.AreEqual(postfix.Single(), payload[1]);
                        }

                        Assert.AreEqual(third, await thirdTask);
                    }
                });
        }

        /// <summary>Tests whether random written payload is read back the same.</summary>
        [TestMethod]
        public void PayloadTest()
        {
            AsyncPump.Run(
                async () =>
                {
#pragma warning disable SA1123 // Do not place regions within elements. Necessary so that tested code snippets can be included in the documentation.
                    #region Payload Test
                    var writtenMessage = new S101Message(0x00, new EmberData(0x01, 0x0A, 0x02));
                    var writtenPayload = new byte[8192];
                    this.Random.NextBytes(writtenPayload);

                    using (var encodedStream = new MemoryStream())
                    {
                        // First we create a writer, which can be used to write multiple messages.
                        // We specify which methods are used to write encoded output and flush it plus the size the internal
                        // buffer should have.
                        var writer = new S101Writer(encodedStream.WriteAsync);

                        // Next we write the message. In return we get a Stream object for the payload.
                        using (var payloadStream =
                            await writer.WriteMessageAsync(writtenMessage, CancellationToken.None))
                        {
                            // Now we write the payload.
                            await payloadStream.WriteAsync(writtenPayload, 0, writtenPayload.Length);
                            await payloadStream.DisposeAsync(CancellationToken.None);
                        }

                        await writer.DisposeAsync(CancellationToken.None);

                        // Reset the encoded stream to the beginning, so that we can read from it.
                        encodedStream.Position = 0;

                        // First we create a reader, which can be used to read multiple messages.
                        // We specify which methods are used to read encoded input.
                        var reader = new S101Reader(encodedStream.ReadAsync);
                        Assert.IsTrue(await reader.ReadAsync(CancellationToken.None)); // Read the first message
                        var readMessage = reader.Message;

                        // Assert the written and read messages are equal
                        Assert.AreEqual(writtenMessage.Slot, readMessage.Slot);
                        Assert.AreEqual(writtenMessage.Command, readMessage.Command);

                        using (var readPayload = new MemoryStream())
                        {
                            await reader.Payload.CopyToAsync(readPayload); // Copy the payload.
                            // Assert that there is only one message
                            Assert.IsFalse(await reader.ReadAsync(CancellationToken.None));
                            CollectionAssert.AreEqual(writtenPayload, readPayload.ToArray());
                        }

                        await reader.DisposeAsync(CancellationToken.None);
                    }
                    #endregion
#pragma warning restore SA1123 // Do not place regions within elements
                });
        }

        /// <summary>Tests <see cref="S101Writer"/> exceptions.</summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Test code.")]
        [TestMethod]
        public void ExceptionTest()
        {
            AsyncPump.Run(
                async () =>
                {
                    new S101Writer((b, o, c, t) => Task.FromResult(false)).Ignore();

                    AssertThrow<ArgumentNullException>(() => new S101Writer(null, 1).Ignore());
                    AssertThrow<ArgumentOutOfRangeException>(
                        () => new S101Writer((b, o, c, t) => Task.FromResult(false), 0).Ignore());

                    var writer = new S101Writer((b, o, c, t) => Task.FromResult(false), 1);
                    await AssertThrowAsync<ArgumentNullException>(
                        () => writer.WriteMessageAsync(null, CancellationToken.None));

                    using (var stream = await writer.WriteMessageAsync(EmberDataMessage, CancellationToken.None))
                    {
                        await AssertThrowAsync<ArgumentNullException>(
                            () => stream.WriteAsync(null, 0, 0, CancellationToken.None));

                        await AssertThrowAsync<ArgumentOutOfRangeException>(
                            () => stream.WriteAsync(new byte[1], -1, 1, CancellationToken.None),
                            () => stream.WriteAsync(new byte[1], 0, -1, CancellationToken.None));

                        await AssertThrowAsync<ArgumentException>(
                            () => stream.WriteAsync(new byte[1], 0, 2, CancellationToken.None),
                            () => stream.WriteAsync(new byte[1], 2, 0, CancellationToken.None),
                            () => stream.WriteAsync(new byte[1], 1, 1, CancellationToken.None));

                        await AssertThrowAsync<NotSupportedException>(
                            () => stream.ReadAsync(null, 0, 0, CancellationToken.None));

                        Assert.IsFalse(stream.CanSeek);
                        AssertThrow<NotSupportedException>(
                            () => stream.Length.Ignore(),
                            () => stream.SetLength(0),
                            () => stream.Position.Ignore(),
                            () => stream.Position = 0,
                            () => stream.Seek(0, SeekOrigin.Begin));

                        await AssertThrowAsync<InvalidOperationException>(() => writer.WriteMessageAsync(
                            new S101Message(0x00, new KeepAliveRequest()), CancellationToken.None));
                        await stream.DisposeAsync(CancellationToken.None);
                        await AssertThrowAsync<ObjectDisposedException>(
                            () => stream.WriteAsync(new byte[] { 2 }, 0, 1, CancellationToken.None));
                    }

                    await writer.DisposeAsync(CancellationToken.None);
                    await AssertThrowAsync<ObjectDisposedException>(() => writer.WriteMessageAsync(
                        new S101Message(0x00, new KeepAliveRequest()), CancellationToken.None));
                });
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static Task<byte> WaitForOutOfFrameByte(S101Reader reader)
        {
            var resultSource = new TaskCompletionSource<byte>();
            EventHandler<OutOfFrameByteReceivedEventArgs> handler = null;

            handler = (s, e) =>
            {
                reader.OutOfFrameByteReceived -= handler;
                resultSource.SetResult(e.Value);
            };

            reader.OutOfFrameByteReceived += handler;
            return resultSource.Task;
        }

        private static async Task<byte[]> Encode(S101Message message, byte[] payload = null)
        {
            using (var asyncStream = new MemoryStream())
            {
                var writer = new S101Writer(
                    async (b, o, c, t) =>
                    {
                        // This makes the read operation truly asynchronous, which helps to improve code coverage.
                        await Task.Delay(1);
                        await asyncStream.WriteAsync(b, o, c, t);
                    },
                    1);

                using (var encodingStream = await writer.WriteMessageAsync(message, CancellationToken.None))
                {
                    Assert.AreEqual(encodingStream == null, payload == null);

                    if (encodingStream != null)
                    {
                        Assert.IsFalse(encodingStream.CanRead);
                        Assert.IsTrue(encodingStream.CanWrite);
                        await encodingStream.WriteAsync(payload, 0, payload.Length, CancellationToken.None);
                        await encodingStream.FlushAsync(CancellationToken.None);
                        await encodingStream.DisposeAsync(CancellationToken.None);
                        Assert.IsFalse(encodingStream.CanWrite);
                        await AssertThrowAsync<ObjectDisposedException>(
                            () => encodingStream.WriteAsync(new byte[] { 0 }, 0, 1, CancellationToken.None));
                        await AssertThrowAsync<ObjectDisposedException>(
                            () => encodingStream.FlushAsync(CancellationToken.None));
                    }
                }

                await writer.DisposeAsync(CancellationToken.None);
                return asyncStream.ToArray();
            }
        }
    }
}
