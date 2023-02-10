////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Ember
{
    using System;
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Xml;

    using Glow;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using UnitTesting;

    /// <summary>Tests <see cref="EmberReader"/>.</summary>
    [TestClass]
    public class EmberReaderTest : TestBase
    {
        /// <summary>Tests with an empty stream.</summary>
        [TestMethod]
        public void EmptyTest()
        {
            using (var stream = new MemoryStream())
            using (var reader = new EmberReader(stream, 1))
            {
                Assert.IsFalse(reader.Read());
            }
        }

        /// <summary>Tests whether unread contents is skipped.</summary>
        [TestMethod]
        public void SkipContentsTest()
        {
            using (var stream = new MemoryStream(new byte[] { 0x60, 0x03, 0x01, 0x01, 0xFF, 0x60, 0x03, 0x01, 0x01, 0x00 }))
            using (var reader = new EmberReader(stream, 1))
            {
                Assert.IsTrue(reader.Read());
                Assert.IsTrue(reader.Read());
                Assert.IsFalse(reader.ReadContentsAsBoolean());
            }

            var original = new byte[64];
            Random.Shared.NextBytes(original);
            byte[] encoded;

            using (var stream = new MemoryStream())
            {
                using (var writer = new EmberWriter(stream))
                {
                    writer.WriteValue(EmberId.CreateApplication(0), original);
                    writer.WriteValue(EmberId.CreateApplication(1), true);
                }

                encoded = stream.ToArray();
            }

            using (var stream = new MemoryStream(encoded))
            using (var reader = new EmberReader(stream, 1))
            {
                Assert.IsTrue(reader.Read());
                Assert.AreEqual(InnerNumber.Octetstring, reader.InnerNumber);
                Assert.IsTrue(reader.Read());
                Assert.AreEqual(InnerNumber.Boolean, reader.InnerNumber);
                Assert.AreEqual(true, reader.ReadContentsAsBoolean());
                Assert.IsFalse(reader.Read());
            }
        }

        /// <summary>Tests Boolean contents.</summary>
        [TestMethod]
        public void BooleanTest()
        {
            AssertDecode(InnerNumber.Boolean, true, 0x60, 0x03, 0x01, 0x01, 0xFF);
            AssertDecode(InnerNumber.Boolean, true, 0x60, 0x03, 0x01, 0x01, 0x01);
            AssertDecode(InnerNumber.Boolean, false, 0x60, 0x03, 0x01, 0x01, 0x00);
        }

        /// <summary>Tests Integer contents.</summary>
        [TestMethod]
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1117:ParametersMustBeOnSameLineOrSeparateLines", Justification = "In this case readability is improved.")]
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1115:ParameterMustFollowComma", Justification = "In this case readability is improved.")]
        public void IntegerTest()
        {
            AssertDecode(InnerNumber.Integer, 1L, 0x60, 0x03, 0x02, 0x01, 0x01);
            AssertDecode(InnerNumber.Integer, -1L, 0x60, 0x03, 0x02, 0x01, 0xFF);
            AssertDecode(InnerNumber.Integer, 127L, 0x60, 0x03, 0x02, 0x01, 0x7F);
            AssertDecode(InnerNumber.Integer, -128L, 0x60, 0x03, 0x02, 0x01, 0x80);
            AssertDecode(InnerNumber.Integer, 128L, 0x60, 0x04, 0x02, 0x02, 0x00, 0x80);
            AssertDecode(InnerNumber.Integer, -129L, 0x60, 0x04, 0x02, 0x02, 0xFF, 0x7F);
            AssertDecode(
                InnerNumber.Integer, long.MaxValue,
                0x60, 0x0A, 0x02, 0x08, 0x7F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF);
            AssertDecode(
                InnerNumber.Integer, long.MinValue,
                0x60, 0x0A, 0x02, 0x08, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00);
        }

        /// <summary>Tests Octetstring contents with lengths &lt; 126.</summary>
        [TestMethod]
        public void ShortOctetstringTest()
        {
            AssertDecode(new byte[0]);
            AssertDecode(this.Randomize(new byte[1]));
            AssertDecode(this.Randomize(new byte[Random.Shared.Next(126)]));
        }

        /// <summary>Tests Octetstring contents with lengths &gt;= 128.</summary>
        [TestMethod]
        public void LongOctetstringTest()
        {
            var contents = new byte[Random.Shared.Next(128, 253)];
            this.Randomize(contents);

            var header =
                new byte[]
                {
                    0x60, 0x81, (byte)(contents.Length + 3), InnerNumber.Octetstring, 0x81, (byte)contents.Length
                };

            AssertDecodeContents(InnerNumber.Octetstring, contents, header, contents);
        }

        /// <summary>Tests Real contents.</summary>
        [TestMethod]
        public void RealTest()
        {
            AssertDecode(InnerNumber.Real, 0.0, 0x60, 0x02, 0x09, 0x00);
            AssertDecode(InnerNumber.Real, double.PositiveInfinity, 0x60, 0x03, 0x09, 0x01, 0x40);
            AssertDecode(InnerNumber.Real, double.NegativeInfinity, 0x60, 0x03, 0x09, 0x01, 0x41);
            AssertDecode(InnerNumber.Real, double.NaN, 0x60, 0x03, 0x09, 0x01, 0x42);

            // Incorrectly encoded special values
            AssertDecode(InnerNumber.Real, double.PositiveInfinity, 0x60, 0x06, 0x09, 0x04, 0x81, 0x04, 0x00, 0x00);
            AssertDecode(InnerNumber.Real, double.NegativeInfinity, 0x60, 0x06, 0x09, 0x04, 0xC1, 0x04, 0x00, 0x00);
            AssertDecode(InnerNumber.Real, double.NaN, 0x60, 0x0C, 0x09, 0x0A, 0x81, 0x04, 0x00, 0x0F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF);

            AssertDecode(InnerNumber.Real, -0.0, 0x60, 0x03, 0x09, 0x01, 0x43);
            AssertDecode(InnerNumber.Real, 1.0, 0x60, 0x05, 0x09, 0x03, 0x80, 0x00, 0x01);
            AssertDecode(InnerNumber.Real, -1.0, 0x60, 0x05, 0x09, 0x03, 0xC0, 0x00, 0x01);
            AssertDecode(InnerNumber.Real, double.MaxValue, 0x60, 0x0C, 0x09, 0x0A, 0x81, 0x03, 0xFF, 0x1F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF);
            AssertDecode(InnerNumber.Real, -double.MaxValue, 0x60, 0x0C, 0x09, 0x0A, 0xC1, 0x03, 0xFF, 0x1F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF);
            AssertDecode(InnerNumber.Real, double.MaxValue, 0x60, 0x0D, 0x09, 0x0B, 0x82, 0x00, 0x03, 0xFF, 0x1F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF);
            AssertDecode(InnerNumber.Real, -double.MaxValue, 0x60, 0x0D, 0x09, 0x0B, 0xC2, 0x00, 0x03, 0xFF, 0x1F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF);
            AssertDecode(InnerNumber.Real, double.MaxValue, 0x60, 0x0D, 0x09, 0x0B, 0x83, 0x02, 0x03, 0xFF, 0x1F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF);
            AssertDecode(InnerNumber.Real, -double.MaxValue, 0x60, 0x0D, 0x09, 0x0B, 0xC3, 0x02, 0x03, 0xFF, 0x1F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF);
        }

        /// <summary>Tests UTF8String contents.</summary>
        [TestMethod]
        public void Utf8StringTest()
        {
            AssertDecode(string.Empty);
            AssertDecode(Guid.NewGuid().ToString());
        }

        /// <summary>Tests Relative object identifier contents.</summary>
        [TestMethod]
        public void RelativeObjectIdentifierTest()
        {
            AssertDecode(new int[] { });

            var relativeObjectIdentifier = new int[Random.Shared.Next(32)];

            for (int index = 0; index < relativeObjectIdentifier.Length; ++index)
            {
                relativeObjectIdentifier[index] = Random.Shared.Next(128);
            }

            AssertDecode(relativeObjectIdentifier);
        }

        /// <summary>Tests container contents.</summary>
        [TestMethod]
        public void ContainerTest()
        {
            Action<EmberReader> assertEqual =
                reader =>
                {
                    Assert.IsFalse(reader.CanReadContents);
                    Assert.ThrowsException<InvalidOperationException>(() => reader.ReadContentsAsObject());
                    Assert.IsTrue(reader.Read());
                    Assert.AreEqual(InnerNumber.EndContainer, reader.InnerNumber);
                    Assert.ThrowsException<InvalidOperationException>(() => reader.OuterId.Ignore());
                };

            AssertDecode(InnerNumber.Sequence, assertEqual, 0x60, 0x80, 0x30, 0x80, 0x00, 0x00, 0x00, 0x00);
            AssertDecode(InnerNumber.Sequence, assertEqual, 0x60, 0x02, 0x30, 0x00);
            AssertDecode(InnerNumber.Set, assertEqual, 0x60, 0x80, 0x31, 0x80, 0x00, 0x00, 0x00, 0x00);
            AssertDecode(InnerNumber.Set, assertEqual, 0x60, 0x02, 0x31, 0x00);
        }

        /// <summary>Tests <see cref="EmberWriter"/> exceptions.</summary>
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1117:ParametersMustBeOnSameLineOrSeparateLines", Justification = "In this case readability is improved.")]
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1115:ParameterMustFollowComma", Justification = "In this case readability is improved.")]
        [TestMethod]
        public void ExceptionTest()
        {
            TestStandardExceptionConstructors<EmberException>();

            Assert.ThrowsException<ArgumentNullException>(() => new EmberReader(null, 1).Dispose());
            AssertEmberException("Incorrect length at position 3.", 0x60, 0x03, 0x0D, 0x01, 0xFF, 0x00);

            using (var stream = new MemoryStream(new byte[] { 0x60, 0x03, 0x01, 0x01, 0xFF }))
            using (var reader = new EmberReader(stream, 1))
            {
                reader.Read();
                Assert.ThrowsException<InvalidOperationException>(() => reader.ReadContentsAsString());
                reader.ReadContentsAsBoolean();
            }

            AssertEmberException(
                "Unexpected Universal class for outer identifier at position 0.", 0x01, 0x03, 0x01, 0x01, 0xFF);

            AssertEmberException("Unexpected end of stream.", 0x60);

            AssertEmberException("Unexpected end of stream.", 0x60, 0x05, 0x04, 0x03, 0xFF, 0xFE);

            AssertEmberException(
                "Unexpected end of stream at position 9.", 0x60, 0x08, 0x30, 0x06, 0x60, 0x03, 0x01, 0x01, 0xFF);

            AssertEmberException("Unexpected End-of-contents identifier at position 2.", 0x60, 0x02, 0x00, 0x00);

            AssertEmberException("Unexpected number in universal identifier at position 2.", 0x60, 0x03, 0x03, 0x01, 0xFF);

            AssertEmberException("Unexpected context-specific or private identifier at position 2.", 0x60, 0x03, 0x83, 0x01, 0xFF);
            AssertEmberException("Unexpected context-specific or private identifier at position 2.", 0x60, 0x03, 0xC3, 0x01, 0xFF);

            AssertEmberException("Unexpected length for End-of-contents identifier at position 0.", 0x00, 0x01);

            AssertEmberException("Unexpected excess End-of-contents identifier at position 0.", 0x00, 0x00);

            AssertEmberException(
                "Unexpected End-of-contents identifier at position 6 for definite length at position 1.",
                0x60, 0x06, 0x30, 0x80, 0x00, 0x00, 0x00, 0x00);

            AssertEmberException("Unexpected constructed encoding at position 2.",  0x60, 0x03, 0x21, 0x01, 0xFF);

            AssertEmberException(
                "Unexpected indefinite length for primitive data value at position 2.", 0x60, 0x03, 0x01, 0x80, 0xFF);

            AssertEmberException("Unexpected encoding for Real at position 4.", 0x60, 0x03, 0x09, 0x01, 0x00);

            AssertEmberException("Incorrect length for Real at position 4.", 0x60, 0x04, 0x09, 0x02, 0x80, 0x00);

            AssertEmberException(
                "The exponent of the Real at position 4 exceeds the expected range.",
                0x60, 0x06, 0x09, 0x04, 0x81, 0x04, 0x01, 0x01);

            AssertEmberException(
                "The exponent of the Real at position 4 exceeds the expected range.",
                0x60, 0x06, 0x09, 0x04, 0x81, 0xFC, 0x01, 0x01);

            AssertEmberException(
                "The mantissa of the Real at position 4 is zero.", 0x60, 0x05, 0x09, 0x03, 0x80, 0x00, 0x00);

            AssertEmberException(
                "The length at position 1 exceeds the expected range.", 0x60, 0x84, 0x80, 0x00, 0x00, 0x00);

            AssertEmberException("Unexpected zero length for integer.", 0x60, 0x02, 0x02, 0x00);

            ReadAll(0x60, 0x0A, 0x02, 0x08, 0x7F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF);
            AssertEmberException(
                "The integer, length or exponent at position 4 exceeds the expected range.",
                0x60, 0x0B, 0x02, 0x09, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF);

            ReadAll(0x60, 0x0A, 0x02, 0x08, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00);
            AssertEmberException(
                "The integer, length or exponent at position 4 exceeds the expected range.",
                0x60, 0x0B, 0x02, 0x09, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00);

            ReadAll(0x7F, 0x87, 0xFF, 0xFF, 0xFF, 0x7F, 0x03, 0x01, 0x01, 0xFF);
            AssertEmberException(
                "The identifier number or subidentifier at position 1 exceeds the expected range.",
                0x7F, 0x8F, 0xFF, 0xFF, 0xFF, 0x7F, 0x03, 0x01, 0x01, 0xFF);
        }

        /// <summary>Reads a real world file.</summary>
        [TestMethod]
        public void ReadFileTest()
        {
            var settings = new XmlWriterSettings() { Indent = true, ConformanceLevel = ConformanceLevel.Fragment };
            var converter = new EmberConverter(GlowTypes.Instance);

            int totalInputLength = 0;
            int totalOutputLength = 0;

            foreach (var resourceName in Assembly.GetExecutingAssembly().GetManifestResourceNames())
            {
                if (resourceName.Contains("Ember.EmberDataPayloads") &&
                    resourceName.EndsWith(".ember", StringComparison.Ordinal))
                {
                    byte[] input;

                    using (var source = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                    using (var destination = new MemoryStream())
                    {
                        source.CopyTo(destination);
                        input = destination.ToArray();
                    }

                    string xml;

                    using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
                    {
                        using (var writer = XmlWriter.Create(stringWriter, settings))
                        {
                            converter.ToXml(input, writer);
                        }

                        xml = stringWriter.ToString();
                    }

                    Console.WriteLine(xml);

                    byte[] output;

                    using (var stringReader = new StringReader(xml))
                    using (var reader = XmlReader.Create(stringReader))
                    {
                        output = converter.FromXml(reader);
                    }

                    totalInputLength += input.Length;
                    totalOutputLength += output.Length;

                    Console.WriteLine("Ratio: {0:P}", (double)output.Length / input.Length);
                    Console.WriteLine();

                    using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
                    {
                        using (var writer = XmlWriter.Create(stringWriter, settings))
                        {
                            converter.ToXml(output, writer);
                        }

                        Assert.AreEqual(xml, stringWriter.ToString());
                    }
                }
            }

            Console.WriteLine("Overall Ratio: {0:P}", (double)totalOutputLength / totalInputLength);
            Console.WriteLine();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static void AssertDecode(byte[] contents) =>
            AssertDecodeContents(InnerNumber.Octetstring, contents, contents);

        private static void AssertDecode(string str) =>
            AssertDecodeContents(InnerNumber.Utf8String, str, Encoding.UTF8.GetBytes(str));

        private static void AssertDecode(int[] contents) =>
            AssertDecodeContents(
                InnerNumber.RelativeObjectIdentifier, contents, contents.Select(id => (byte)id).ToArray());

        private static void AssertDecodeContents(int expectedInnerNumber, object expectedValue, byte[] contents)
        {
            var header =
                new byte[] { 0x60, (byte)(contents.Length + 2), (byte)expectedInnerNumber, (byte)contents.Length };
            AssertDecodeContents(expectedInnerNumber, expectedValue, header, contents);
        }

        private static void AssertDecodeContents(
            int expectedInnerNumber, object expectedValue, byte[] header, byte[] contents)
        {
            using (var stream = new MemoryStream())
            {
                stream.Write(header, 0, header.Length);
                stream.Write(contents, 0, contents.Length);

                var expectedCollection = expectedValue as ICollection;

                if (expectedCollection == null)
                {
                    AssertDecode(expectedInnerNumber, expectedValue, stream.ToArray());
                }
                else
                {
                    AssertDecode(expectedInnerNumber, expectedCollection, CollectionAssert.AreEqual, stream.ToArray());
                }
            }
        }

        private static void AssertDecode(int expectedInnerNumber, object expectedValue, params byte[] input) =>
            AssertDecode(expectedInnerNumber, expectedValue, Assert.AreEqual, input);

        private static void AssertDecode<T>(
            int expectedInnerNumber, T expectedValue, Action<T, T> assertEqual, params byte[] input)
        {
            AssertDecode(
                expectedInnerNumber,
                reader =>
                {
                    Assert.IsTrue(reader.CanReadContents);
                    assertEqual(expectedValue, (T)reader.ReadContentsAsObject());
                },
                input);
        }

        private static void AssertDecode(int expectedInnerNumber, Action<EmberReader> assertEqual, params byte[] input)
        {
            using (var stream = new MemoryStream(input))
            using (var reader = new EmberReader(stream, 1))
            {
                Assert.ThrowsException<InvalidOperationException>(() => reader.InnerNumber.GetHashCode().Ignore());
                Assert.ThrowsException<InvalidOperationException>(() => reader.OuterId.Ignore());
                Assert.ThrowsException<InvalidOperationException>(() => reader.ReadContentsAsObject());
                Assert.IsFalse(reader.CanReadContents);
                Assert.IsTrue(reader.Read());
                Assert.AreEqual(EmberId.CreateApplication(0), reader.OuterId);
                Assert.AreEqual(expectedInnerNumber, reader.InnerNumber);
                assertEqual(reader);
                Assert.ThrowsException<InvalidOperationException>(() => reader.ReadContentsAsObject());
                Assert.IsFalse(reader.Read());

                reader.Dispose();
                Assert.IsFalse(reader.CanReadContents);
                Assert.ThrowsException<ObjectDisposedException>(() => reader.InnerNumber.Ignore());
                Assert.ThrowsException<ObjectDisposedException>(() => reader.OuterId.Ignore());
                Assert.ThrowsException<ObjectDisposedException>(() => reader.ReadContentsAsObject());
            }

            using (var writer = XmlWriter.Create(Console.Out, new XmlWriterSettings() { Indent = true }))
            {
                new EmberConverter(GlowTypes.Instance).ToXml(input, writer);
                Console.WriteLine();
                Console.WriteLine();
            }
        }

        private static void AssertEmberException(string message, params byte[] input)
        {
            try
            {
                ReadAll(input);
                Assert.Fail("EmberException was not thrown.");
            }
            catch (EmberException ex)
            {
                Assert.AreEqual(message, ex.Message);
            }
        }

        private static void ReadAll(params byte[] input)
        {
            using (var stream = new MemoryStream(input))
            using (var reader = new EmberReader(stream, 1))
            {
                while (reader.Read())
                {
                    switch (reader.InnerNumber)
                    {
                        case InnerNumber.EndContainer:
                            Assert.ThrowsException<InvalidOperationException>(() => reader.OuterId.Ignore());
                            Assert.IsFalse(reader.CanReadContents);
                            Assert.ThrowsException<InvalidOperationException>(() => reader.ReadContentsAsObject());
                            break;
                        case InnerNumber.Sequence:
                        case InnerNumber.Set:
                            reader.OuterId.Ignore();
                            Assert.IsFalse(reader.CanReadContents);
                            Assert.ThrowsException<InvalidOperationException>(() => reader.ReadContentsAsObject());
                            break;
                        case InnerNumber.Boolean:
                        case InnerNumber.Integer:
                        case InnerNumber.Octetstring:
                        case InnerNumber.Real:
                        case InnerNumber.Utf8String:
                        case InnerNumber.RelativeObjectIdentifier:
                            reader.OuterId.Ignore();
                            Assert.IsTrue(reader.CanReadContents);
                            reader.ReadContentsAsObject();
                            break;
                        default:
                            Assert.IsTrue(reader.InnerNumber >= InnerNumber.FirstApplication);
                            reader.OuterId.Ignore();
                            Assert.IsFalse(reader.CanReadContents);
                            Assert.ThrowsException<InvalidOperationException>(() => reader.ReadContentsAsObject());
                            break;
                    }
                }
            }
        }

        private byte[] Randomize(byte[] bytes)
        {
            Random.Shared.NextBytes(bytes);
            return bytes;
        }
    }
}
