////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using Lawo.EmberPlusSharpTest;

namespace Lawo.EmberPlusSharp.Model
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Tests <see cref="ElementAttribute"/>.</summary>
    [TestClass]
    public sealed class ElementAttributeTest
    {
        /// <summary>Tests the main use cases.</summary>
        [TestMethod]
        public void MainTest()
        {
            var identifier = TestHelper.RandomString();
            var isOptional = TestHelper.RandomBoolean();
            var attribute = new ElementAttribute() { Identifier = identifier, IsOptional = isOptional };
            Assert.AreEqual(identifier, attribute.Identifier);
            Assert.AreEqual(isOptional, attribute.IsOptional);
            Assert.ThrowsException<ArgumentNullException>(() => new ElementAttribute() { Identifier = null }.Ignore());
        }
    }
}
