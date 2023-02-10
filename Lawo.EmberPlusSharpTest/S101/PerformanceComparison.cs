////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.S101
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using EmberLib.Glow.Framing;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Threading.Tasks;

    /// <summary>Compares the performance of this implementation with the one of EmberLib.net.</summary>
    [TestClass]
    public class PerformanceComparison : CommunicationTestBase
    {
        /// <summary>Measures <see cref="S101Reader"/> asynchronous performance.</summary>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", Justification = "Text references code elements.")]
        [TestMethod]
        public void S101ReadTest()
        {
            AsyncPump.Run(
                async () =>
                {
                    byte[] input;

                    using (var stream = new MemoryStream())
                    {
                        var writer = new S101Writer(stream.WriteAsync, 1024);

                        using (var payloadStream = await writer.WriteMessageAsync(EmberDataMessage, CancellationToken.None))
                        {
                            var payload = new byte[BlockSize];
                            Random.Shared.NextBytes(payload);
                            await payloadStream.WriteAsync(payload, 0, payload.Length);
                            await payloadStream.DisposeAsync(CancellationToken.None);
                        }

                        await writer.DisposeAsync(CancellationToken.None);
                        input = stream.ToArray();
                    }

                    Console.WriteLine(
                        "S101Reader asynchronous: {0}ms",
                        await TimeMethod(count => TestS101ReaderAsync(input, count), LoopCount));
                });
        }

        /// <summary>Compares <see cref="S101Writer"/> performance with <see cref="GlowOutput"/> performance.</summary>
        /// <remarks>For both classes conditions are exactly the same:
        /// <list type="bullet">
        /// <item>The input consists of a block of <see cref="BlockSize"/> random bytes written <see cref="BlockCount"/>
        /// times.</item>
        /// <item>The output is simply ignored.</item>
        /// </list>
        /// These conditions ensure that most memory accesses can be satisfied by the cache (i.e. memory bandwidth
        /// should not be a limiting factor) and that the time to write the output can be ignored.</remarks>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", Justification = "Text references code elements.")]
        [TestMethod]
        public void S101WriteTest()
        {
            AsyncPump.Run(
                async () =>
                {
                    var input = new byte[BlockSize];
                    Random.Shared.NextBytes(input);
                    var glowOutputMilliseconds = await TimeMethod(count => TestGlowOutput(input, count), LoopCount);
                    var s101WriterAsyncMilliseconds = await TimeMethod(count => TestS101WriterAsync(input, count), LoopCount);

                    Console.WriteLine("GlowOutput: {0}ms", glowOutputMilliseconds);
                    Console.WriteLine("S101Writer asynchronous: {0}ms", s101WriterAsyncMilliseconds);
                    Console.WriteLine("Ratio: {0}", (double)glowOutputMilliseconds / s101WriterAsyncMilliseconds);
                });
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private const int BlockSize = 4096;
        private const int BlockCount = 4096;
        private const int LoopCount = 3;

        private static async Task TestS101ReaderAsync(byte[] message, int messageCount)
        {
            byte[] buffer = new byte[BlockSize];

            using (var stream = new MemoryStream(message))
            {
                var reader = new S101Reader(stream.ReadAsync, 1024);

                for (int index = 0; index < messageCount; ++index)
                {
                    await reader.ReadAsync(CancellationToken.None);

                    using (var payload = reader.Payload)
                    {
                        await payload.ReadAsync(buffer, 0, buffer.Length);
                        await payload.DisposeAsync(CancellationToken.None);
                    }

                    stream.Position = 0;
                }

                await reader.DisposeAsync(CancellationToken.None);
            }
        }

        private static Task TestGlowOutput(byte[] block, int blockCount)
        {
            using (var output = new GlowOutput(1024, 0, (s, e) => { }))
            {
                for (int index = 0; index < blockCount; ++index)
                {
                    output.WriteBytes(block);
                }

                output.Finish();
            }

            return Task.FromResult(false);
        }

        private static async Task TestS101WriterAsync(byte[] block, int blockCount)
        {
            var completed = Task.FromResult(false);
            var writer = new S101Writer((b, o, c, t) => completed, 1024);

            using (var payloadStream = await writer.WriteMessageAsync(EmberDataMessage, CancellationToken.None))
            {
                for (int index = 0; index < blockCount; ++index)
                {
                    await payloadStream.WriteAsync(block, 0, block.Length);
                }

                await payloadStream.DisposeAsync(CancellationToken.None);
            }

            await writer.DisposeAsync(CancellationToken.None);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.GC.Collect", Justification = "Necessary for performance testing.")]
        private static async Task<long> TimeMethod(Func<int, Task> method, int loopCount)
        {
            await method(1); // Make sure everything is JITed
            var stopwatch = new Stopwatch();

            for (var current = 0; current < loopCount; ++current)
            {
                GC.Collect();
                stopwatch.Start();
                await method(BlockCount);
                GC.Collect(); // Make sure that the impact of all allocations is measured.
                stopwatch.Stop();
            }

            return stopwatch.ElapsedMilliseconds / loopCount;
        }
    }
}
