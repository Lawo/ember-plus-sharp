////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.S101
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using System.Xml;

    using Lawo.EmberPlus.Ember;
    using Lawo.EmberPlus.S101.EmberDataPayloads;
    using Lawo.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Tests <see cref="S101Client"/>.</summary>
    [TestClass]
    public class S101RobotTest : CommunicationTestBase
    {
        private static readonly EmberTypeBag Types = new EmberTypeBag();

        /// <summary>Tests the main use cases of <see cref="S101Robot"/>.</summary>
        [TestCategory("Unattended")]
        [TestMethod]
        public void SkipTest()
        {
            AsyncPump.Run(() => TestWithRobot<S101Payloads>(
                Types,
                "SkipLog.xml",
                true,
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
                null));
        }

        /// <summary>Tests <see cref="S101Robot"/> use cases for incoming messages.</summary>
        [TestCategory("Unattended")]
        [TestMethod]
        public void IncomingTest()
        {
            AsyncPump.Run(() => AssertThrowAsync<S101Exception>(() => TestWithRobot<S101Payloads>(
                Types,
                "IncomingLog.xml",
                false,
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
                null)));
        }

        /// <summary>Tests what happens when the S101 connection is lost prematurely.</summary>
        [TestCategory("Unattended")]
        [TestMethod]
        public void ConnectionLostTest()
        {
            AsyncPump.Run(() => AssertThrowAsync<S101Exception>(() => TestWithRobot<S101Payloads>(
                Types,
                "IncomingLog.xml",
                false,
                client =>
                {
                    client.Dispose();
                    return Task.FromResult(false);
                },
                null,
                null)));
        }

        /// <summary>Tests <see cref="S101Robot"/> exceptions.</summary>
        [TestCategory("Unattended")]
        [TestMethod]
        public void ExceptionTest()
        {
            AsyncPump.Run(
                async () =>
                {
                    using (var client = new S101Client(Stream.Null, Stream.Null.ReadAsync, Stream.Null.WriteAsync))
                    {
                        await AssertThrowAsync<ArgumentNullException>(
                            () => S101Robot.RunAsync(null, Types, XmlReader.Create(Stream.Null), false),
                            () => S101Robot.RunAsync(client, null, XmlReader.Create(Stream.Null), false),
                            () => S101Robot.RunAsync(client, Types, null, false));
                    }

                    await AssertThrowAsync<XmlException>(() => TestWithRobot<S101Payloads>(
                        Types, "MissingPayloadLog.xml", true, client => Task.FromResult(false), null, null));
                });
        }
    }
}
