////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.Linq.Expressions
{
    using System.Reflection;
    using Lawo.UnitTesting;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Tests the <see cref="ExpressionHelper"/> class.</summary>
    [TestClass]
    public sealed class ExpressionHelperTest : TestBase
    {
        /// <summary>Tests the main use cases.</summary>
        [TestCategory("Unattended")]
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
