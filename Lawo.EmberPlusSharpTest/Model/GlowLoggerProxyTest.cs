////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System.Net.Sockets;
    using System.Reflection;
    using System.Xml;

    using Glow;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using S101;
    using Test;
    using Test.EmberDataPayloads;
    using Threading.Tasks;

    /// <summary>Tests Glow Logger Proxy.</summary>
    [TestClass]
    public class GlowLoggerProxyTest : CommunicationTestBase
    {
        /// <summary>Tests the main use cases.</summary>
        [TestMethod]
        [TestCategory("Manual")]
        public void MainTest()
        {
            var cancelToken = new CancellationTokenSource().Token;
            AsyncPump.Run(
                async () =>
                {
                    var proTask = WaitForConnectionAsync(9000);

                    using (var conTcp = new TcpClient())
                    {
                        await conTcp.ConnectAsync("localhost", 8999);

                        using (var proTcp = await proTask)
                        {
                            var proStream = proTcp.GetStream();

                            using (var proS101 = new S101Client(proTcp, proStream.ReadAsync, proStream.WriteAsync))
                            {
                                var conStream = conTcp.GetStream();

                                using (var conS101 = new S101Client(conTcp, conStream.ReadAsync, conStream.WriteAsync))
                                using (var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                                    typeof(ModelPayloads), "SapphireLog.xml"))
                                using (var reader = XmlReader.Create(resourceStream))
                                {
                                    var robotTask = S101Robot.RunAsync(proS101, GlowTypes.Instance, reader, true);

                                    using (await Consumer<EmptyDynamicRoot>.CreateAsync(conS101))
                                    {
                                        await conS101.SendOutOfFrameByteAsync(0x00);
                                        await robotTask;
                                    }
                                }
                            }
                        }
                    }
                },
                cancelToken);
        }
    }
}
