////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.Linq.Expressions
{
    using System.Reflection;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using UnitTesting;

    /// <summary>Tests the <see cref="ExpressionHelper"/> class.</summary>
    [TestClass]
    public sealed class ExpressionHelperTest : TestBase
    {
        /// <summary>Tests the main use cases.</summary>
        [TestMethod]
        public void MainTest()
        {
            var conventional = typeof(ExpressionHelperTest).GetProperty(
                "SomeProperty", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.AreEqual(conventional, ExpressionHelper.GetPropertyInfo((ExpressionHelperTest t) => t.SomeProperty));
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private int SomeProperty { get; set; }
    }
}
