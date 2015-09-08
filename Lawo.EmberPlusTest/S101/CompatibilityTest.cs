////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.S101
{
    using System.IO;
    using System.Threading;
    using EmberLib.Glow.Framing;
    using Lawo.IO;
    using Lawo.Threading.Tasks;
    using Lawo.UnitTesting;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Tests compatibility with EmberLib.net.</summary>
    [TestClass]
    public class CompatibilityTest : TestBase
    {
        /// <summary>Tests whether random data written with <see cref="GlowOutput"/> is read back the same with
        /// <see cref="S101Reader"/>.</summary>
        [TestCategory("Unattended")]
        [TestMethod]
        public void MainTest()
        {
            AsyncPump.Run(
                async () =>
                {
                    var writtenBytes = new byte[this.Random.Next(3072, 10241)];
                    this.Random.NextBytes(writtenBytes);

                    using (var output = new MemoryStream())
                    {
                        using (var framer = new GlowOutput(1024, 0, (s, e) => output.Write(e.FramedPackage, 0, e.FramedPackageLength)))
                        {
                            framer.WriteBytes(writtenBytes);
                            framer.Finish();
                        }

                        output.Position = 0;
                        var reader = new S101Reader((ReadAsyncCallback)output.ReadAsync, 1024);
                        Assert.IsTrue(await reader.ReadAsync(CancellationToken.None));
                        Assert.IsInstanceOfType(reader.Message.Command, typeof(EmberData));

                        using (var input = new MemoryStream())
                        {
                            await reader.Payload.CopyToAsync(input);
                            CollectionAssert.AreEqual(writtenBytes, input.ToArray());
                        }

                        await reader.DisposeAsync(CancellationToken.None);
                    }
                });
        }
    }
}
