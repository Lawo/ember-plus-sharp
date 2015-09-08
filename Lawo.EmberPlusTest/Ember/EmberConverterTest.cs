////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Ember
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Xml;
    using Lawo.EmberPlus.Ember.EmberDataPayloads;
    using Lawo.EmberPlus.Glow;
    using Lawo.EmberPlus.S101;
    using Lawo.Threading.Tasks;
    using Lawo.UnitTesting;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Tests <see cref="EmberConverter"/>.</summary>
    [TestClass]
    public class EmberConverterTest : TestBase
    {
        /// <summary>Tests <see cref="EmberConverter"/> exceptions.</summary>
        [TestCategory("Unattended")]
        [TestMethod]
        public void ExceptionTest()
        {
            AssertThrow<ArgumentNullException>(
                () => new EmberType(null).ToString(),
                () => new EmberTypeBag(null).ToString(),
                () => new EmberConverter(null).ToString());

            using (var stream = new MemoryStream())
            using (var reader = new EmberReader(stream))
            using (var writer = XmlWriter.Create(new StringBuilder()))
            {
                var converter = new EmberConverter();
                AssertThrow<ArgumentNullException>(
                    () => converter.ToXml((byte[])null, writer),
                    () => converter.ToXml(new byte[0], null),
                    () => converter.ToXml((EmberReader)null, writer),
                    () => converter.ToXml(reader, null));
            }

            using (var stringReader = new StringReader(string.Empty))
            using (var reader = XmlReader.Create(stringReader))
            using (var stream = new MemoryStream())
            using (var writer = new EmberWriter(stream))
            {
                var converter = new EmberConverter();
                AssertThrow<ArgumentNullException>(
                    () => converter.FromXml(null),
                    () => converter.FromXml(null, writer),
                    () => converter.FromXml(reader, null));
            }

            AssertXmlException("<whatever type=\"A-11\"></whatever>", "Unknown field path: whatever.");
            AssertXmlException(
                "<A-0 type=\"Sequence\"><whatever type=\"Set\" /></A-0>", "Unknown field path: A-0.whatever.");
            AssertXmlException("<A-0 type=\"C-11\"></A-0>", "Unknown type: C-11.");
            AssertXmlException(
                "<A-0 type=\"A-11\" whatever=\"\"></A-0>",
                "Unexpected Attribute Count: Each element must have exactly one type attribute.");
            AssertXmlException(
                "<A-0 type=\"A-11\"><![CDATA[Whatever]]></A-0>",
                "Unexpected Node Type: Encountered CDATA while looking for Element.");
            AssertXmlException("<A-0 type=\"Boolean\" />", "Unexpected empty element for a field of type Boolean.");
        }

        /// <summary>Tests that <see cref="EmberConverter"/> can handle invalid XML characters.</summary>
        [TestCategory("Unattended")]
        [TestMethod]
        public void InvalidXmlCharactersTest()
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new EmberWriter(stream))
                {
                    writer.WriteValue(EmberId.CreateContextSpecific(0), "\0");
                }

                var builder = new StringBuilder();

                using (var xmlWriter = XmlWriter.Create(builder))
                {
                    var converter = new EmberConverter();
                    converter.ToXml(stream.ToArray(), xmlWriter);
                }
            }
        }

        /// <summary>Tests <see cref="EmberConverter"/> performance.</summary>
        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", Justification = "Test code.")]
        [TestCategory("Unattended")]
        [TestMethod]
        public void PerformanceTest()
        {
            byte[] payload;

            using (var memoryStream = new MemoryStream())
            {
                using (var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    typeof(EmberPayloads), "BigPayload.ember"))
                {
                    resourceStream.CopyTo(memoryStream);
                }

                payload = memoryStream.ToArray();
            }

            var stopwatch = new Stopwatch();
            var converter = new EmberConverter(GlowTypes.Instance);
            GC.Collect();
            GC.WaitForPendingFinalizers();

            for (int index = 0; index < 100; ++index)
            {
                using (var writer = XmlWriter.Create(TextWriter.Null))
                {
                    stopwatch.Start();

                    using (var stream = new MemoryStream(payload))
                    using (var reader = new EmberReader(stream))
                    {
                        converter.ToXml(reader, writer);
                    }

                    stopwatch.Stop();
                }
            }

            stopwatch.Start();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            stopwatch.Stop();

            Console.WriteLine(stopwatch.ElapsedMilliseconds);
        }

        /// <summary>Tests whatever.</summary>
        [TestMethod]
        public void SimonTest()
        {
            AsyncPump.Run(
                async () =>
                {
                    using (var rawStream = new MemoryStream())
                    {
                        using (var reader = File.OpenText(@"C:\Users\hubera01\Desktop\message.txt"))
                        {
                            var data = reader.ReadToEnd();

                            for (var index = 0; index < data.Length; index += 2)
                            {
                                rawStream.WriteByte(Convert.ToByte(data.Substring(index, 2), 16));
                            }
                        }

                        rawStream.Position = 0;
                        var s101Reader = new S101Reader(rawStream.ReadAsync);
                        var converter = new EmberConverter(GlowTypes.Instance);
                        var buffer = new byte[1024];

                        while (await s101Reader.ReadAsync(CancellationToken.None))
                        {
                            if (s101Reader.Message.Command is EmberData)
                            {
                                using (var payloadStream = s101Reader.Payload)
                                using (var stream = new MemoryStream())
                                {
                                    int read;

                                    while ((read = await payloadStream.ReadAsync(buffer, 0, buffer.Length, CancellationToken.None)) > 0)
                                    {
                                        stream.Write(buffer, 0, read);
                                    }

                                    stream.Position = 0;

                                    using (var emberReader = new EmberReader(stream))
                                    using (var writer = XmlWriter.Create(@"C:\Users\hubera01\Desktop\dump.xml", new XmlWriterSettings() { Indent = true, ConformanceLevel = ConformanceLevel.Fragment }))
                                    {
                                        converter.ToXml(emberReader, writer);
                                    }
                                }
                            }
                        }
                    }
                });
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static void AssertXmlException(string xml, string expectedMessage)
        {
            using (var stringReader = new StringReader(xml))
            using (var reader = XmlReader.Create(stringReader))
            {
                var converter = new EmberConverter();

                try
                {
                    converter.FromXml(reader);
                    throw new ArgumentException(
                        string.Format(CultureInfo.InvariantCulture, "Unexpected success with xml {0}.", xml), "xml");
                }
                catch (XmlException ex)
                {
                    Assert.AreEqual(expectedMessage, ex.Message);
                }
            }
        }
    }
}
