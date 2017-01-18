////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.IO
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using UnitTesting;

    /// <summary>Tests <see cref="BufferStream"/>.</summary>
    [TestClass]
    public class BufferStreamTest : TestBase
    {
        /// <summary>Tests less executed <see cref="WriteBuffer"/> use cases.</summary>
        [TestMethod]
        public void CanReadCanWriteTest()
        {
            var readBuffer = this.Random.Next(2) == 1 ? new ReadBuffer((b, o, c) => 0, 1024) : null;
            var writeBuffer = this.Random.Next(2) == 1 ? new WriteBuffer((b, o, c) => { }, 1024) : null;

            using (var stream = new MyBufferStream(readBuffer, writeBuffer))
            {
                Assert.AreEqual(readBuffer != null, stream.CanRead);
                Assert.AreEqual(writeBuffer != null, stream.CanWrite);
                stream.Dispose();
                Assert.IsFalse(stream.CanRead);
                Assert.IsFalse(stream.CanWrite);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private sealed class MyBufferStream : BufferStream
        {
            internal MyBufferStream(ReadBuffer readBuffer, WriteBuffer writeBuffer)
                : base(readBuffer, writeBuffer)
            {
            }
        }
    }
}
