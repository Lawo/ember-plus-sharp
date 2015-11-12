////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System.Linq;
    using System.Reflection;

    using Lawo.UnitTesting;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Tests <see cref="StreamDescription"/>.</summary>
    [TestClass]
    public sealed class StreamDescriptionTest : TestBase
    {
        /// <summary>Tests the main use cases.</summary>
        [TestMethod]
        public void MainTest()
        {
            var descr1 = (StreamDescription)typeof(StreamDescription).GetTypeInfo().DeclaredConstructors.First().Invoke(
                new object[] { StreamFormat.Int16BigEndian, 10 });
            TestStructEquality(descr1, new StreamDescription(), (l, r) => l == r, (l, r) => l != r);
        }
    }
}
