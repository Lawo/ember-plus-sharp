////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.ComponentModel
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Reflection;
    using UnitTesting;

    /// <summary>Tests <see cref="Trigger"/>.</summary>
    [TestClass]
    public sealed class TriggerTest : TestBase
    {
        /// <summary>Tests the main <see cref="Trigger"/> use cases.</summary>
        [TestMethod]
        public void MainTest()
        {
            var callCount = 0;
            var source = new Source();

            using (Trigger.Create(source.GetProperty(o => o.Value), p => ++callCount))
            {
                ++source.Value;
                Assert.AreEqual(1, callCount);
                ++source.Value;
                Assert.AreEqual(2, callCount);
            }

            ++source.Value;
            Assert.AreEqual(2, callCount);
        }

        /// <summary>Tests the exceptional <see cref="Trigger"/> use cases.</summary>
        [TestMethod]
        public void ExceptionTest() =>
            AssertThrow<ArgumentNullException>(
                () => Trigger.Create((IProperty<Source, int>)null, p => { }).Dispose(),
                () => Trigger.Create(new Source().GetProperty(o => o.Value), null).Dispose());

        private sealed class Source : NotifyPropertyChanged
        {
            internal int Value
            {
                get { return this.val; }
                set { this.SetValue(ref this.val, value); }
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////

            private int val;
        }
    }
}
