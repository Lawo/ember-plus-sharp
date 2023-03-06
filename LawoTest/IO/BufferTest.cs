////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.IO
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Threading.Tasks;
    using UnitTesting;

    /// <summary>Tests <see cref="ReadBuffer"/> and <see cref="WriteBuffer"/>.</summary>
    [TestClass]
    public class BufferTest : TestBase
    {
        /// <summary>Tests less executed <see cref="ReadBuffer"/> use cases.</summary>
        [TestMethod]
        public void ReadTest()
        {
            var originalbytes = new byte[3];
            this.Random.NextBytes(originalbytes);

            // This covers the case where the read bytes are copied into the buffer in two chunks
            using (var originalStream = new MemoryStream(originalbytes))
            {
                var readBuffer = new ReadBuffer(originalStream.Read, 2);
                var buffer = new byte[1];
                int read;

                using (var readStream = new MemoryStream())
                {
                    while ((read = readBuffer.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        readStream.Write(buffer, 0, read);
                    }

                    Assert.AreEqual(0, readBuffer.Read(new byte[readBuffer.Capacity + 1], 0, readBuffer.Capacity + 1));
                    CollectionAssert.AreEqual(originalbytes, readStream.ToArray());
                }
            }
        }

        /// <summary>Tests less executed <see cref="ReadBuffer"/> use cases.</summary>
        [TestMethod]
        public void ReadAsyncTest()
        {
            var cancelToken = new CancellationTokenSource().Token;
            AsyncPump.Run(
                async () =>
                {
                    var originalBytes = new byte[3];
                    this.Random.NextBytes(originalBytes);

                    // This covers the case where the read bytes are copied into the buffer in two chunks
                    using (var originalStream = new MemoryStream(originalBytes))
                    {
                        var readBuffer = new ReadBuffer((ReadAsyncCallback)originalStream.ReadAsync, 2);
                        var buffer = new byte[1];
                        int read;

                        using (var readStream = new MemoryStream())
                        {
                            while ((read = await readBuffer.ReadAsync(
                                buffer, 0, buffer.Length, CancellationToken.None)) > 0)
                            {
                                readStream.Write(buffer, 0, read);
                            }

                            var largeBuffer = new byte[readBuffer.Capacity + 1];
                            Assert.AreEqual(
                                0,
                                await readBuffer.ReadAsync(largeBuffer, 0, largeBuffer.Length, CancellationToken.None));
                            CollectionAssert.AreEqual(originalBytes, readStream.ToArray());
                        }
                    }
                }, cancelToken);
        }

        /// <summary>Tests <see cref="ReadBuffer.FillAsync(byte[], int, int, CancellationToken)"/>.</summary>
        [TestMethod]
        public void FillAsyncTest()
        {
            var cancelToken = new CancellationTokenSource().Token;
            AsyncPump.Run(
                async () =>
                {
                    var originalBytes = new byte[3];
                    this.Random.NextBytes(originalBytes);

                    using (var originalStream = new MemoryStream(originalBytes))
                    {
                        var readBuffer = new ReadBuffer((ReadAsyncCallback)originalStream.ReadAsync, 2);
                        var readBytes = new byte[originalBytes.Length];
                        await readBuffer.FillAsync(readBytes, 0, readBytes.Length, CancellationToken.None);
                        CollectionAssert.AreEqual(originalBytes, readBytes);
                    }
                },
                cancelToken);
        }

        /// <summary>Tests less executed <see cref="WriteBuffer"/> use cases.</summary>
        [TestMethod]
        public void WriteTest()
        {
            var originalBytes = new byte[2];
            this.Random.NextBytes(originalBytes);

            // This covers the case where the written bytes are copied into the buffer in two chunks
            using (var stream = new MemoryStream())
            {
                var writeBuffer = new WriteBuffer(stream.Write, 1);
                writeBuffer.Write(originalBytes, 0, originalBytes.Length);
                writeBuffer.Flush();
                CollectionAssert.AreEqual(originalBytes, stream.ToArray());
            }
        }

        /// <summary>Tests less executed <see cref="WriteBuffer"/> use cases.</summary>
        [TestMethod]
        public void WriteAsyncTest()
        {
            var cancelToken = new CancellationTokenSource().Token;
            AsyncPump.Run(
                async () =>
                {
                    var bytes = new byte[2];
                    this.Random.NextBytes(bytes);

                    // This covers the case where the written bytes are copied into the buffer in two chunks
                    using (var stream = new MemoryStream())
                    {
                        var writeBuffer = new WriteBuffer(stream.WriteAsync, 1);
                        await writeBuffer.WriteAsync(bytes, 0, bytes.Length, CancellationToken.None);
                        await writeBuffer.FlushAsync(CancellationToken.None);
                        CollectionAssert.AreEqual(bytes, stream.ToArray());
                    }
                },
                cancelToken);
        }

        /// <summary>Tests various exceptions.</summary>
        [TestMethod]
        public void ExceptionTest()
        {
            var cancelToken = new CancellationTokenSource().Token;
            AsyncPump.Run(
                async () =>
                {
                    AssertThrow<ArgumentNullException>(
                        () => new ReadBuffer((ReadCallback)null, 1).Ignore(),
                        () => new ReadBuffer((ReadAsyncCallback)null, 1).Ignore(),
                        () => new WriteBuffer((WriteCallback)null, 1).Ignore(),
                        () => new WriteBuffer((WriteAsyncCallback)null, 1).Ignore(),
                        () => new WriteBuffer((b, o, c) => { }, 1).WriteAsUtf8(null, 0));
                    AssertThrow<ArgumentOutOfRangeException>(() => new ReadBuffer((b, o, c) => 0, 0).Ignore());

                    using (var stream = new MemoryStream())
                    {
                        var readBuffer = new ReadBuffer(stream.Read, 1);
                        AssertThrow<EndOfStreamException>(
                            () => readBuffer.Fill(new byte[1], 0, 1),
                            () => readBuffer.Fill(new byte[2], 0, 2));
                        await AssertThrowAsync<InvalidOperationException>(
                            () => readBuffer.ReadAsync(CancellationToken.None),
                            () => readBuffer.ReadAsync(new byte[1], 0, 1, CancellationToken.None),
                            () => readBuffer.FillAsync(1, CancellationToken.None),
                            () => readBuffer.FillAsync(new byte[1], 0, 1, CancellationToken.None));

                        var asyncReadBuffer = new ReadBuffer((ReadAsyncCallback)stream.ReadAsync, 1);
                        await AssertThrowAsync<EndOfStreamException>(
                            () => asyncReadBuffer.FillAsync(new byte[1], 0, 1, CancellationToken.None),
                            () => asyncReadBuffer.FillAsync(new byte[2], 0, 2, CancellationToken.None));
                        AssertThrow<InvalidOperationException>(
                            () => asyncReadBuffer.Read(),
                            () => asyncReadBuffer.Read(new byte[1], 0, 1),
                            () => asyncReadBuffer.Fill(1),
                            () => asyncReadBuffer.Fill(new byte[1], 0, 1),
                            () => asyncReadBuffer.ReadUtf8(1));

                        var writeBuffer = new WriteBuffer(stream.Write, 1);
                        await AssertThrowAsync<InvalidOperationException>(
                            () => writeBuffer.FlushAsync(CancellationToken.None),
                            () => writeBuffer.ReserveAsync(2, CancellationToken.None),
                            () => writeBuffer.WriteAsync(new byte[3], 0, 3, CancellationToken.None));

                        var asyncWriteBuffer = new WriteBuffer(stream.WriteAsync, 1);
                        asyncWriteBuffer[asyncWriteBuffer.Count++] = 42;
                        AssertThrow<InvalidOperationException>(() => asyncWriteBuffer.Flush());
                        asyncWriteBuffer[asyncWriteBuffer.Count++] = 42;
                        AssertThrow<InvalidOperationException>(
                            () => asyncWriteBuffer.Reserve(2), () => asyncWriteBuffer.Write(new byte[3], 0, 3));
                        asyncWriteBuffer[asyncWriteBuffer.Count++] = 42;
                        var str = "Hello";
                        AssertThrow<InvalidOperationException>(
                            () => asyncWriteBuffer.WriteAsUtf8(str, Encoding.UTF8.GetByteCount(str)));
                    }
                },
                cancelToken);
        }
    }
}
