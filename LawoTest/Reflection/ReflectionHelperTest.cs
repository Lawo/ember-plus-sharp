////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.Reflection
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using UnitTesting;

    /// <summary>Tests the <see cref="ReflectionHelper"/> class.</summary>
    [TestClass]
    public sealed class ReflectionHelperTest : TestBase
    {
        /// <summary>Initializes a new instance of the <see cref="ReflectionHelperTest"/> class.</summary>
        public ReflectionHelperTest()
        {
            this.someField = this.Random.Next();
        }

        /// <summary>Tests the main use cases.</summary>
        [TestMethod]
        public void MainTest()
        {
            var property = this.GetProperty(o => o.SomeProperty);
            var value = this.Random.Next();
            this.SomeProperty = value;
            Assert.AreEqual(value, property.Value);
            Assert.AreEqual(value, this.SomeProperty);
            value = this.Random.Next();
            property.Value = value;
            Assert.AreEqual(value, property.Value);
            Assert.AreEqual(value, this.SomeProperty);
        }

        /// <summary>Tests exceptional paths.</summary>
        [TestMethod]
        public void ExceptionTest()
        {
            AssertThrow<ArgumentNullException>(
                () => ((ReflectionHelperTest)null).GetProperty(o => o.SomeProperty).Ignore(),
                () => this.GetProperty<ReflectionHelperTest, int>(null));
            AssertThrow<ArgumentException>(
                () => this.GetProperty(o => 1),
                () => this.GetProperty(o => o.someField));

            var property = this.GetProperty(o => o.SomeReadOnlyProperty);
            Assert.AreEqual(this.someField, property.Value);
            AssertThrow<InvalidOperationException>(() => property.Value = 42);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private readonly int someField;

        private int SomeProperty { get; set; }

        private int SomeReadOnlyProperty => this.someField;
    }
}
