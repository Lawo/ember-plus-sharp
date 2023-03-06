////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.S101
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Xml;

    using Ember;
    using EmberDataPayloads;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Tests <see cref="S101Client"/>.</summary>
    [TestClass]
    public class S101LogReaderTest : CommunicationTestBase
    {
        /// <summary>Tests the main use cases of <see cref="S101LogReader"/>.</summary>
        [TestMethod]
        public void SkipTest()
        {
            TestS101LogReader(
                "SkipLog.xml",
                reader =>
                {
                    int outOfFrameByteCount = 0;
                    int dataCount = 0;
                    int requestCount = 0;
                    int responseCount = 0;

                    while (reader.Read())
                    {
                        Assert.AreNotEqual(DateTime.Today, reader.TimeUtc);
                        Assert.AreEqual(DateTimeKind.Utc, reader.TimeUtc.Kind);
                        Assert.IsFalse(string.IsNullOrWhiteSpace(reader.Direction));

                        switch (reader.EventType)
                        {
                            case "Message":
                                Assert.IsFalse(reader.Number == 0);
                                Assert.IsNotNull(reader.Message);

                                if (reader.Message.Command is EmberData)
                                {
                                    ++dataCount;
                                }
                                else if (reader.Message.Command is KeepAliveRequest)
                                {
                                    ++requestCount;
                                }
                                else if (reader.Message.Command is KeepAliveResponse)
                                {
                                    ++responseCount;
                                }
                                else
                                {
                                    Assert.Fail("Unknown command.");
                                }

                                break;
                            case "OutOfFrameByte":
                                Assert.AreEqual(0, reader.Number);
                                Assert.IsNull(reader.Message);
                                var payload = reader.GetPayload();
                                Assert.AreEqual(1, payload.Length);
                                ++outOfFrameByteCount;
                                break;
                            default:
                                Assert.Fail("Unknown event type.");
                                break;
                        }
                    }

                    Assert.AreEqual(1, outOfFrameByteCount);
                    Assert.AreEqual(3, dataCount);
                    Assert.AreEqual(1, requestCount);
                    Assert.AreEqual(1, responseCount);
                });
        }

        /// <summary>Tests <see cref="S101Robot"/> exceptions.</summary>
        [TestMethod]
        public void ExceptionTest()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => new S101LogReader(null, XmlReader.Create(Stream.Null)).Ignore());
            Assert.ThrowsException<ArgumentNullException>(
                () => new S101LogReader(Types, null).Ignore());
            Assert.ThrowsException<InvalidOperationException>(
                () => TestS101LogReader("IncomingLog.xml", r => r.Direction.Ignore()));
            Assert.ThrowsException<XmlException>(() => TestS101LogReader("MissingPayloadLog.xml", r => r.Read()));
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static readonly EmberTypeBag Types = new EmberTypeBag();

        private static void TestS101LogReader(string logXmlName, Action<S101LogReader> testCallback)
        {
            using (var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                typeof(S101Payloads), logXmlName))
            using (var reader = XmlReader.Create(resourceStream))
            {
                testCallback(new S101LogReader(Types, reader));
            }
        }
    }
}
