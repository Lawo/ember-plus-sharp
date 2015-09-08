////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model
{
    using System;

    using Lawo.UnitTesting;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Tests <see cref="ElementAttribute"/>.</summary>
    [TestClass]
    public sealed class ElementAttributeTest : TestBase
    {
        /// <summary>Tests the main use cases.</summary>
        [TestCategory("Unattended")]
        [TestMethod]
        public void MainTest()
        {
            var identifier = GetRandomString();
            var isOptional = this.GetRandomBoolean();
            var attribute = new ElementAttribute() { Identifier = identifier, IsOptional = isOptional };
            Assert.AreEqual(identifier, attribute.Identifier);
            Assert.AreEqual(isOptional, attribute.IsOptional);
            AssertThrow<ArgumentNullException>(() => new ElementAttribute() { Identifier = null });
        }
    }
}
