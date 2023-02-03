////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.S101
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using System.Xml;

    using Ember;
    using EmberDataPayloads;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Threading.Tasks;

    /// <summary>Tests <see cref="S101Client"/>.</summary>
    [TestClass]
    public class S101RobotTest : CommunicationTestBase
    {
        /// <summary>Tests the main use cases of <see cref="S101Robot"/>.</summary>
        [TestMethod]
        public void SkipTest()
        {
            var cancelToken = new CancellationTokenSource().Token;
            AsyncPump.Run(() => TestWithRobot<S101Payloads>(
                async client =>
                {
                    var done = new TaskCompletionSource<bool>();
                    var count = 0;

                    client.EmberDataReceived += (s, e) =>
                        {
                            if (++count == 3)
                            {
                                done.SetResult(true);
                            }
                        };

                    await done.Task;
                },
                null,
                null,
                Types,
                true,
                "SkipLog.xml"),
                cancelToken);
        }

        /// <summary>Tests <see cref="S101Robot"/> use cases for incoming messages.</summary>
        [TestMethod]
        public void IncomingTest()
        {
            var cancelToken = new CancellationTokenSource().Token;
            AsyncPump.Run(() => Assert.ThrowsExceptionAsync<S101Exception>(() => TestWithRobot<S101Payloads>(
                async client =>
                {
                    using (var stream = new MemoryStream())
                    {
                        using (var writer = new EmberWriter(stream))
                        {
                            writer.WriteValue(EmberId.CreateApplication(0), false);
                        }

                        await client.SendMessageAsync(EmberDataMessage, stream.ToArray());
                    }
                },
                null,
                null,
                Types,
                false,
                "IncomingLog.xml")),
                cancelToken);
        }

        /// <summary>Tests what happens when the S101 connection is lost prematurely.</summary>
        [TestMethod]
        public void ConnectionLostTest()
        {
            var cancelToken = new CancellationTokenSource().Token;
            AsyncPump.Run(() => Assert.ThrowsExceptionAsync<S101Exception>(() => TestWithRobot<S101Payloads>(
                client =>
                {
                    client.Dispose();
                    return Task.FromResult(false);
                },
                null,
                null,
                Types,
                false,
                "IncomingLog.xml")),
                cancelToken);
        }

        /// <summary>Tests <see cref="S101Robot"/> exceptions.</summary>
        [TestMethod]
        public void ExceptionTest()
        {
            var cancelToken = new CancellationTokenSource().Token;
            AsyncPump.Run(
                async () =>
                {
                    using (var client = new S101Client(Stream.Null, Stream.Null.ReadAsync, Stream.Null.WriteAsync))
                    {
                        await Assert.ThrowsExceptionAsync<ArgumentNullException>(
                            () => S101Robot.RunAsync(null, Types, XmlReader.Create(Stream.Null), false));
                        await Assert.ThrowsExceptionAsync<ArgumentNullException>(
                            () => S101Robot.RunAsync(client, null, XmlReader.Create(Stream.Null), false));
                        await Assert.ThrowsExceptionAsync<ArgumentNullException>(
                            () => S101Robot.RunAsync(client, Types, null, false));
                    }

                    await Assert.ThrowsExceptionAsync<XmlException>(() => TestWithRobot<S101Payloads>(
                        client => Task.FromResult(false), null, null, Types, true, "MissingPayloadLog.xml"));
                },
                cancelToken);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static readonly EmberTypeBag Types = new EmberTypeBag();
    }
}
