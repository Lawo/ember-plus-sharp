////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model
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
        [TestCategory("Unattended")]
        [TestMethod]
        public void MainTest()
        {
            var descr1 = (StreamDescription)typeof(StreamDescription).GetTypeInfo().DeclaredConstructors.First().Invoke(
                new object[] { StreamFormat.Int16BigEndian, 10 });
            TestStructEquality(descr1, new StreamDescription(), (l, r) => l == r, (l, r) => l != r);
        }
    }
}
