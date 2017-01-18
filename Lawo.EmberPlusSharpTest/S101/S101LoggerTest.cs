////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.S101
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Xml;
    using Ember;
    using Glow;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using UnitTesting;

    /// <summary>Tests <see cref="S101Logger"/>.</summary>
    [TestClass]
    public class S101LoggerTest : TestBase
    {
        /// <summary>Tests the main use cases.</summary>
        [TestMethod]
        public void MainTest()
        {
            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                AssertThrow<ArgumentNullException>(
                    () => new S101Logger(null, writer).Dispose(),
                    () => new S101Logger(GlowTypes.Instance, (TextWriter)null).Dispose(),
                    () => new S101Logger(GlowTypes.Instance, (XmlWriter)null).Dispose(),
                    () => new S101Logger((IEmberConverter)null, XmlWriter.Create(writer)).Dispose());

                using (var logger = new S101Logger(GlowTypes.Instance, writer))
                {
                    AssertThrow<ArgumentNullException>(
                        () => logger.LogData("Whatever", "Send", null, 0, 0),
                        () => logger.LogMessage(null, new S101Message(0x00, new KeepAliveRequest()), null),
                        () => logger.LogMessage("Send", null, null),
                        () => logger.LogException("Send", null));
                }
            }
        }
    }
}
