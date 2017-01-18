////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Ember
{
    using System;
    using System.IO;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using UnitTesting;

    /// <summary>Tests <see cref="EmberId"/>.</summary>
    [TestClass]
    public class IdentifierTest : TestBase
    {
        /// <summary>Tests the main use cases.</summary>
        [TestMethod]
        public void MainTest()
        {
            var u1 = default(EmberId);
            var n1 = this.Random.Next();
            var n2 = n1 + 1;
            var a1 = EmberId.CreateApplication(n1);
            var a2 = EmberId.CreateApplication(n2);
            var c1 = EmberId.CreateContextSpecific(n1);
            EmberId p1;

            using (var stream = new MemoryStream(new byte[] { 0xE0, 0x03, 0x01, 0x01, 0xFF }))
            using (var reader = new EmberReader(stream, 1))
            {
                reader.Read();
                p1 = reader.OuterId;
            }

            TestStructEquality(a1, a2, (l, r) => l == r, (l, r) => l != r);
            TestStructEquality(a1, c1, (l, r) => l == r, (l, r) => l != r);

            TestParse(u1);
            TestParse(a1);
            TestParse(c1);
            TestParse(p1);

            EmberId dummy;
            Assert.IsFalse(EmberId.TryParse("S-234", out dummy));
            Assert.IsFalse(EmberId.TryParse("U+234", out dummy));
            Assert.IsFalse(EmberId.TryParse("P--234", out dummy));
            Assert.IsFalse(EmberId.TryParse("A-89345734579385749354", out dummy));
        }

        /// <summary>Tests <see cref="EmberId"/> exceptions.</summary>
        [TestMethod]
        public void ExceptionTest() =>
            AssertThrow<ArgumentOutOfRangeException>(
                () => EmberId.CreateApplication(-1),
                () => EmberId.CreateContextSpecific(-1));

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static void TestParse(EmberId emberId)
        {
            EmberId parsed;
            Assert.IsTrue(EmberId.TryParse(emberId.ToString(), out parsed));
            Assert.AreEqual(emberId, parsed);
            Console.WriteLine(emberId);
        }
    }
}
