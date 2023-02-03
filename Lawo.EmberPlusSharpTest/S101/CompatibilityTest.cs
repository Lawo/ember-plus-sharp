////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.S101
{
    using System.IO;
    using System.Threading;
    using EmberLib.Glow.Framing;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Threading.Tasks;
    using UnitTesting;

    /// <summary>Tests compatibility with EmberLib.net.</summary>
    [TestClass]
    public class CompatibilityTest : TestBase
    {
        /// <summary>Tests whether random data written with <see cref="GlowOutput"/> is read back the same with
        /// <see cref="S101Reader"/>.</summary>
        [TestMethod]
        public void MainTest()
        {
            var cancelToken = new CancellationTokenSource().Token;
            AsyncPump.Run(
                async () =>
                {
                    var writtenBytes = new byte[Random.Shared.Next(3072, 10241)];
                    Random.Shared.NextBytes(writtenBytes);

                    using (var output = new MemoryStream())
                    {
                        using (var framer = new GlowOutput(1024, 0, (s, e) => output.Write(e.FramedPackage, 0, e.FramedPackageLength)))
                        {
                            framer.WriteBytes(writtenBytes);
                            framer.Finish();
                        }

                        output.Position = 0;
                        var reader = new S101Reader(output.ReadAsync, 1024);
                        Assert.IsTrue(await reader.ReadAsync(CancellationToken.None));
                        Assert.IsInstanceOfType(reader.Message.Command, typeof(EmberData));

                        using (var input = new MemoryStream())
                        {
                            await reader.Payload.CopyToAsync(input);
                            CollectionAssert.AreEqual(writtenBytes, input.ToArray());
                        }

                        await reader.DisposeAsync(CancellationToken.None);
                    }
                },
                cancelToken);
        }
    }
}
