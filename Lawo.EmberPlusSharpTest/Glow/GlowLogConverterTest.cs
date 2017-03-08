////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Glow
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Xml;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Model.Test.EmberDataPayloads;
    using S101;

    /// <summary>Tests <see cref="S101Client"/>.</summary>
    [TestClass]
    public class GlowLogConverterTest : CommunicationTestBase
    {
        /// <summary>Tests the main use cases of <see cref="GlowLogConverter"/>.</summary>
        [TestMethod]
        public void MainTest()
        {
            TestS101LogConverter("MainLog.xml");
            TestS101LogConverter("FunctionLog.xml");
            TestS101LogConverter("IsOnlineLog.xml");
            TestS101LogConverter("MatrixMainLog.xml");
        }

        /// <summary>Tests <see cref="GlowLogConverter"/> exceptions.</summary>
        [TestMethod]
        public void ExceptionTest() =>
            AssertThrow<ArgumentNullException>(
                () => GlowLogConverter.Convert(null, XmlWriter.Create(Stream.Null)),
                () => GlowLogConverter.Convert(XmlReader.Create(Stream.Null), null));

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static void TestS101LogConverter(string logXmlName)
        {
            var settings = new XmlWriterSettings { Indent = true, CloseOutput = true };

            using (var stream =
                Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(ModelPayloads), logXmlName))
            using (var reader = XmlReader.Create(stream))
            using (var writer = File.CreateText(Path.ChangeExtension(logXmlName, ".Converted.xml")))
            using (var xmlwriter = XmlWriter.Create(writer, settings))
            {
                GlowLogConverter.Convert(reader, xmlwriter);
            }
        }
    }
}
