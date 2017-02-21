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

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Threading.Tasks;
    using UnitTesting;

    /// <summary>Tests <see cref="MessageEncodingStream"/>.</summary>
    [TestClass]
    public class S101ReaderTest : TestBase
    {
        /// <summary>Tests <see cref="S101Reader.ReadAsync"/>.</summary>
        [TestMethod]
        public void ReadTest()
        {
            AsyncPump.Run(
                async () =>
                {
                    await AssertDecode<KeepAliveRequest>(new byte[] { 0xFE, 0x00, 0x0E, 0x01, 0x01, 0x94, 0xE4, 0xFF });
                    await AssertDecode<KeepAliveResponse>(new byte[] { 0xFE, 0x00, 0x0E, 0x02, 0x01, 0xFD, 0xDC, 0xCE, 0xFF });
                    await AssertDecode<EmberData>(
                        new byte[]
                        {
                            0xFE, 0x00, 0x0E, 0x00, 0x01, 0x80, 0x01, 0x02, 0x0A, 0x02, 0xF5, 0x78, 0xFF,
                            0xFE, 0x00, 0x0E, 0x00, 0x01, 0x60, 0x01, 0x02, 0x0A, 0x02, 0x13, 0x53, 0xFF
                        });
                });
        }

        /// <summary>Tests <see cref="S101Reader"/> exceptions.</summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Test code.")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", Justification = "Literals name code elements.")]
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1117:ParametersMustBeOnSameLineOrSeparateLines", Justification = "In this case readability is improved.")]
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1115:ParameterMustFollowComma", Justification = "In this case readability is improved.")]
        [TestMethod]
        public void ExceptionTest()
        {
            TestStandardExceptionConstructors<S101Exception>();

            AsyncPump.Run(
                async () =>
                {
                    new S101Reader((b, o, c, t) => Task.FromResult(0)).Ignore();

                    AssertThrow<ArgumentNullException>(() => new S101Reader(null, 1).Ignore());
                    AssertThrow<ArgumentOutOfRangeException>(() => new S101Reader((b, o, c, t) => Task.FromResult(0), 0).Ignore());

                    using (var input = new MemoryStream(
                        new byte[] { 0xFE, 0x00, 0x0E, 0x01, 0x01, 0x94, 0xE4, 0xFF, 0xFE, 0x00, 0x0E, 0x02, 0x01, 0xFD, 0xDC, 0xCE, 0xFF }))
                    {
                        var reader = new S101Reader(input.ReadAsync, 1);
                        AssertThrow<InvalidOperationException>(() => reader.Message.Ignore());
                        AssertThrow<InvalidOperationException>(() => reader.Payload.Ignore());
                        Assert.IsTrue(await reader.ReadAsync(CancellationToken.None));
                        Assert.IsInstanceOfType(reader.Message.Command, typeof(KeepAliveRequest));
                        Assert.AreEqual(0, await reader.Payload.ReadAsync(new byte[1], 0, 1, CancellationToken.None));
                        AssertThrow<NotSupportedException>(
                            () => reader.Payload.Read(new byte[1], 0, 1),
                            () => reader.Payload.Write(new byte[1], 0, 1));
                        Assert.IsTrue(await reader.ReadAsync(CancellationToken.None));
                        Assert.IsInstanceOfType(reader.Message.Command, typeof(KeepAliveResponse));
                        Assert.AreEqual(0, await reader.Payload.ReadAsync(new byte[1], 0, 1, CancellationToken.None));
                        Assert.IsFalse(await reader.ReadAsync(CancellationToken.None));
                        AssertThrow<InvalidOperationException>(() => reader.Message.Ignore());
                        AssertThrow<InvalidOperationException>(() => reader.Payload.Ignore());
                        await reader.DisposeAsync(CancellationToken.None);
                        await AssertThrowAsync<ObjectDisposedException>(() => reader.ReadAsync(CancellationToken.None));
                        AssertThrow<ObjectDisposedException>(
                            () => reader.Message.Ignore(), () => reader.Payload.Ignore());
                    }

                    await AssertEmpty(0xFE, 0xFF);
                    await AssertEmpty(0xFE, 0xFE);

                    for (byte invalid = 0xF8; invalid < 0xFD; ++invalid)
                    {
                        await AssertEmpty(0xFE, invalid);
                    }

                    for (ushort invalid = 0xF8; invalid < 0x100; ++invalid)
                    {
                        await AssertEmpty(0xFE, 0xFD, (byte)invalid);
                    }

                    await AssertS101Exception("Unexpected end of stream.", 0xFE, 0x00, 0x00, 0x00);

                    await AssertS101Exception("Unexpected end of stream.", 0xFE, 0x00, 0x0E, 0x00, 0x00, 0x00);

                    await AssertS101Exception(
                        "Unexpected end of stream.",
                        0xFE, 0x00, 0x0E, 0x00, 0x01, 0x80, 0x01, 0x02, 0x0A, 0x02, 0xF5, 0x78, 0xFF);

                    await AssertS101Exception(
                        "Inconsistent Slot in multi-packet message.",
                        0xFE, 0x00, 0x0E, 0x00, 0x01, 0x80, 0x01, 0x02, 0x0A, 0x02, 0xF5, 0x78, 0xFF,
                        0xFE, 0x01, 0x0E, 0x00, 0x01, 0x60, 0x01, 0x02, 0x0A, 0x02, 0x00, 0x00, 0x00);

                    await AssertS101Exception(
                        "Unexpected Message Type.",
                        0xFE, 0x00, 0x0F, 0x00, 0x01, 0x80, 0x01, 0x02, 0x0A, 0x02, 0x00, 0x00, 0x00);

                    await AssertS101Exception(
                        "Unexpected Command.", 0xFE, 0x00, 0x0E, 0x04, 0x01, 0x80, 0x01, 0x02, 0x0A, 0x02, 0x00, 0x00, 0x00);

                    await AssertS101Exception(
                        "Inconsistent Command in multi-packet message.",
                        0xFE, 0x00, 0x0E, 0x00, 0x01, 0x80, 0x01, 0x02, 0x0A, 0x02, 0xF5, 0x78, 0xFF,
                        0xFE, 0x00, 0x0E, 0x01, 0x01, 0x60, 0x01, 0x02, 0x0A, 0x02, 0x00, 0x00, 0x00);

                    await AssertS101Exception(
                        "Unexpected Version.", 0xFE, 0x00, 0x0E, 0x00, 0x00, 0x80, 0x01, 0x02, 0x0A, 0x02, 0x00, 0x00, 0x00);

                    await AssertS101Exception(
                        "Missing FirstPacket flag in first packet.",
                        0xFE, 0x00, 0x0E, 0x00, 0x01, 0x20, 0x01, 0x02, 0x0A, 0x02, 0x00, 0x00, 0x00);

                    await AssertS101Exception(
                        "FirstPacket flag in subsequent packet.",
                        0xFE, 0x00, 0x0E, 0x00, 0x01, 0x80, 0x01, 0x02, 0x0A, 0x02, 0xF5, 0x78, 0xFF,
                        0xFE, 0x00, 0x0E, 0x00, 0x01, 0xE0, 0x01, 0x02, 0x0A, 0x02, 0x00, 0x00, 0x00);
                });
        }

        /// <summary>Tests <see cref="S101Command"/> methods.</summary>
        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(new KeepAliveRequest(), new KeepAliveRequest());
            Assert.AreNotEqual(new KeepAliveRequest(), new KeepAliveResponse());
            Assert.AreEqual(new KeepAliveRequest().GetHashCode(), new KeepAliveRequest().GetHashCode());
            Assert.AreNotEqual(new KeepAliveRequest().GetHashCode(), new KeepAliveResponse().GetHashCode());
        }

        /// <summary>Tests <see cref="S101Message"/> methods.</summary>
        [TestMethod]
        public void MessageTest() => AssertThrow<ArgumentNullException>(() => new S101Message(0x00, null).Ignore());

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static async Task AssertDecode<TCommand>(byte[] bytes, byte[] expectedPayload = null, byte slot = 0x00)
            where TCommand : S101Command
        {
            using (var actualPayload = new MemoryStream())
            {
                var reader = CreateAsyncReader(bytes);
                var dummyBuffer = new byte[] { 42 };

                Assert.IsTrue(await reader.ReadAsync(CancellationToken.None));
                var message = reader.Message;
                var decodingStream = reader.Payload;
                Assert.IsFalse(decodingStream.CanWrite);
                await AssertThrowAsync<NotSupportedException>(
                    () => decodingStream.WriteAsync(dummyBuffer, 0, dummyBuffer.Length, CancellationToken.None));
                Assert.IsTrue(decodingStream.CanRead);
                Assert.AreEqual(slot, message.Slot);
                Assert.IsFalse(message.Command.Equals(null));
                Assert.IsInstanceOfType(message.Command, typeof(TCommand));

                await decodingStream.CopyToAsync(actualPayload);

                if (expectedPayload == null)
                {
                    Assert.AreEqual(0, actualPayload.Length);
                }
                else
                {
                    CollectionAssert.AreEqual(expectedPayload, actualPayload.ToArray());
                }

                await decodingStream.DisposeAsync(CancellationToken.None);
                Assert.IsFalse(decodingStream.CanRead);
                await AssertThrowAsync<ObjectDisposedException>(
                    () => decodingStream.ReadAsync(dummyBuffer, 0, dummyBuffer.Length, CancellationToken.None));
                await reader.DisposeAsync(CancellationToken.None);
            }
        }

        private static async Task AssertEmpty(params byte[] input)
        {
            var reader = CreateAsyncReader(input);
            Assert.IsFalse(await reader.ReadAsync(CancellationToken.None));
            await reader.DisposeAsync(CancellationToken.None);
        }

        private static async Task AssertS101Exception(string message, params byte[] input)
        {
            var reader = CreateAsyncReader(input);
            var buffer = new byte[1024];

            try
            {
                await reader.ReadAsync(CancellationToken.None);
            }
            catch (S101Exception ex)
            {
                Assert.AreEqual(message, ex.Message);
                return;
            }

            try
            {
                while (await reader.Payload.ReadAsync(buffer, 0, buffer.Length, CancellationToken.None) > 0)
                {
                }

                Assert.Fail("S101Exception was not thrown.");
            }
            catch (S101Exception ex)
            {
                Assert.AreEqual(message, ex.Message);
            }

            try
            {
                // Subsequent attempts to Read must all be answered with the same exception
                // (message may be different)
                await reader.Payload.ReadAsync(buffer, 0, buffer.Length, CancellationToken.None);
                Assert.Fail("S101Exception was not thrown.");
            }
            catch (S101Exception)
            {
            }

            // We're intentionally not calling reader.DisposeAsync here, as that will cause another S101Exception
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Test code.")]
        private static S101Reader CreateAsyncReader(byte[] bytes)
        {
            var stream = new MemoryStream(bytes);

            return new S101Reader(
                async (b, o, c, t) =>
                {
                    // This makes the read operation truly asynchronous, which helps to improve code coverage.
                    await Task.Delay(1);
                    return await stream.ReadAsync(b, o, c, t);
                },
                1);
        }
    }
}
