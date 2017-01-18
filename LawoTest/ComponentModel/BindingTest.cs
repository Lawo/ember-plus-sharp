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

    /// <summary>Tests <see cref="Binding{T, U, V, W}"/>.</summary>
    [TestClass]
    public sealed class BindingTest : TestBase
    {
        /// <summary>Initializes the members for testing.</summary>
        [TestInitialize]
        public void Initialize()
        {
            this.source = new Source();
            this.sourceNotifyCount = 0;
            this.target = new Target();
            this.targetNotifyCount = 0;
            this.source.PropertyChanged += (s, e) => this.sourceNotifyCount += e.PropertyName == "Property" ? 1 : 0;
            this.target.PropertyChanged += (s, e) => this.targetNotifyCount += e.PropertyName == "Property" ? 1 : 0;
        }

        /// <summary>Tests the main <see cref="TwoWayBinding"/> use cases.</summary>
        [TestMethod]
        public void TwoWayBindingTest()
        {
            this.AssertValues(this.source.Property, 0, this.target.Property, 0);
            Assert.AreNotEqual(this.source.Property, this.target.Property);
            var value = this.source.Property;
            int sourceOriginated = 0;
            int targetOriginated = 0;

            using (var binding = TwoWayBinding.Create(
                this.source.GetProperty(o => o.Property), this.target.GetProperty(o => o.Property)))
            {
                binding.ChangeOriginatedAtSource +=
                    (s, e) =>
                    {
                        Assert.AreEqual(binding, s);
                        Assert.AreEqual(binding.Source, e.Property);
                        ++sourceOriginated;
                    };

                binding.ChangeOriginatedAtTarget +=
                    (s, e) =>
                    {
                        Assert.AreEqual(binding, s);
                        Assert.AreEqual(binding.Target, e.Property);
                        ++targetOriginated;
                    };

                this.AssertValues(value, 0, value, 1);
                AssertOriginatedCounts(0, sourceOriginated, 0, targetOriginated);
                this.source.Property = value = GetRandomString();
                this.AssertValues(value, 1, value, 2);
                AssertOriginatedCounts(1, sourceOriginated, 0, targetOriginated);
                this.target.Property = value = GetRandomString();
                this.AssertValues(value, 2, value, 3);
                AssertOriginatedCounts(1, sourceOriginated, 1, targetOriginated);
            }

            this.AssertValues(value, 2, value, 3);
            AssertOriginatedCounts(1, sourceOriginated, 1, targetOriginated);

            this.source.Property.Ignore();
            var targetValue = this.target.Property;
            var sourceValue = this.source.Property = GetRandomString();
            this.AssertValues(sourceValue, 3, targetValue, 3);
            AssertOriginatedCounts(1, sourceOriginated, 1, targetOriginated);
            this.target.Property = targetValue = GetRandomString();
            this.AssertValues(sourceValue, 3, targetValue, 4);
            AssertOriginatedCounts(1, sourceOriginated, 1, targetOriginated);
        }

        /// <summary>Tests the <see cref="OneWayBinding"/> use cases not yet tested with <see cref="TwoWayBinding"/>.
        /// </summary>
        [TestMethod]
        public void OneWayBindingTest()
        {
            var value = this.source.Property;
            int sourceOriginated = 0;
            int targetOriginated = 0;

            using (var binding = OneWayBinding.Create(
                this.source.GetProperty(o => o.Property), this.target.GetProperty(o => o.Property)))
            {
                binding.ChangeOriginatedAtSource += (s, e) => ++sourceOriginated;
                binding.ChangeOriginatedAtTarget += (s, e) => ++targetOriginated;

                this.AssertValues(value, 0, value, 1);
                AssertOriginatedCounts(0, sourceOriginated, 0, targetOriginated);
                this.source.Property = value = GetRandomString();
                this.AssertValues(value, 1, value, 2);
                AssertOriginatedCounts(1, sourceOriginated, 0, targetOriginated);
                string targetValue = GetRandomString();
                this.target.Property = targetValue;
                this.AssertValues(value, 1, targetValue, 3);
                AssertOriginatedCounts(1, sourceOriginated, 0, targetOriginated);
            }
        }

        /// <summary>Tests <see cref="OneWayBinding"/> and <see cref="TwoWayBinding"/> exceptions.</summary>
        [TestMethod]
        public void ExceptionTest()
        {
            AssertThrow<ArgumentNullException>(
                () => TwoWayBinding.Create((IProperty<Source, string>)null, this.target.GetProperty(o => o.Property)).Dispose(),
                () => TwoWayBinding.Create(this.source.GetProperty(o => o.Property), (IProperty<Target, string>)null).Dispose(),
                () => TwoWayBinding.Create(this.source.GetProperty(o => o.Property), null, this.target.GetProperty(o => o.Property), v => v).Dispose(),
                () => TwoWayBinding.Create(this.source.GetProperty(o => o.Property), v => v, this.target.GetProperty(o => o.Property), null).Dispose());
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static void AssertOriginatedCounts(
            int expectedSourceOriginated, int sourceOriginated, int expectedTargetOriginated, int targetOriginated)
        {
            Assert.AreEqual(expectedSourceOriginated, sourceOriginated);
            Assert.AreEqual(expectedTargetOriginated, targetOriginated);
        }

        private Source source;
        private int sourceNotifyCount;
        private Target target;
        private int targetNotifyCount;

        private void AssertValues(
            string expectedSourceValue,
            int expectedSourceNotifyCount,
            string expectedTargetValue,
            int expectedTargetNotifyCount)
        {
            Assert.AreEqual(expectedSourceValue, this.source.Property);
            Assert.AreEqual(expectedSourceNotifyCount, this.sourceNotifyCount);
            Assert.AreEqual(expectedTargetValue, this.target.Property);
            Assert.AreEqual(expectedTargetNotifyCount, this.targetNotifyCount);
        }

        private sealed class Source : NotifyPropertyChanged
        {
            internal string Property
            {
                get { return this.property; }
                set { this.SetValue(ref this.property, value); }
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////

            private string property = Guid.NewGuid().ToString();
        }

        private sealed class Target : NotifyPropertyChanged
        {
            internal string Property
            {
                get { return this.property; }
                set { this.SetValue(ref this.property, value); }
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////

            private string property = Guid.NewGuid().ToString();
        }
    }
}
