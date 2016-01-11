////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2016 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.ComponentModel
{
    using System;
    using Lawo.Reflection;
    using Lawo.UnitTesting;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Tests <see cref="Binding{T, U, V, W}"/>.</summary>
    [TestClass]
    public sealed class BindingTest : TestBase
    {
        private Source source;
        private int sourceNotifyCount;
        private Target target;
        private int targetNotifyCount;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

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
            AssertValues(this.source.Property, 0, this.target.Property, 0);
            Assert.AreNotEqual(this.source.Property, this.target.Property);
            var value = source.Property;
            int sourceOriginated = 0;
            int targetOriginated = 0;

            using (var binding =
                TwoWayBinding.Create(source.GetProperty(o => o.Property), target.GetProperty(o => o.Property)))
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

                AssertValues(value, 0, value, 1);
                AssertOriginatedCounts(0, sourceOriginated, 0, targetOriginated);
                source.Property = value = GetRandomString();
                AssertValues(value, 1, value, 2);
                AssertOriginatedCounts(1, sourceOriginated, 0, targetOriginated);
                target.Property = value = GetRandomString();
                AssertValues(value, 2, value, 3);
                AssertOriginatedCounts(1, sourceOriginated, 1, targetOriginated);
            }

            AssertValues(value, 2, value, 3);
            AssertOriginatedCounts(1, sourceOriginated, 1, targetOriginated);

            var sourceValue = source.Property;
            var targetValue = target.Property;
            source.Property = sourceValue = GetRandomString();
            AssertValues(sourceValue, 3, targetValue, 3);
            AssertOriginatedCounts(1, sourceOriginated, 1, targetOriginated);
            target.Property = targetValue = GetRandomString();
            AssertValues(sourceValue, 3, targetValue, 4);
            AssertOriginatedCounts(1, sourceOriginated, 1, targetOriginated);
        }

        /// <summary>Tests the <see cref="OneWayBinding"/> use cases not yet tested with <see cref="TwoWayBinding"/>.
        /// </summary>
        [TestMethod]
        public void OneWayBindingTest()
        {
            var value = source.Property;
            int sourceOriginated = 0;
            int targetOriginated = 0;

            using (var binding =
                OneWayBinding.Create(source.GetProperty(o => o.Property), target.GetProperty(o => o.Property)))
            {
                binding.ChangeOriginatedAtSource += (s, e) => ++sourceOriginated;
                binding.ChangeOriginatedAtTarget += (s, e) => ++targetOriginated;

                AssertValues(value, 0, value, 1);
                AssertOriginatedCounts(0, sourceOriginated, 0, targetOriginated);
                source.Property = value = GetRandomString();
                AssertValues(value, 1, value, 2);
                AssertOriginatedCounts(1, sourceOriginated, 0, targetOriginated);
                string targetValue = GetRandomString();
                target.Property = targetValue;
                AssertValues(value, 1, targetValue, 3);
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
                () => TwoWayBinding.Create(this.source.GetProperty(o => o.Property), (Func<string, string>)null, this.target.GetProperty(o => o.Property), v => v).Dispose(),
                () => TwoWayBinding.Create(this.source.GetProperty(o => o.Property), v => v, this.target.GetProperty(o => o.Property), (Func<string, string>)null).Dispose());
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

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

        private static void AssertOriginatedCounts(
            int expectedSourceOriginated, int sourceOriginated, int expectedTargetOriginated, int targetOriginated)
        {
            Assert.AreEqual(expectedSourceOriginated, sourceOriginated);
            Assert.AreEqual(expectedTargetOriginated, targetOriginated);
        }

        private sealed class Source : NotifyPropertyChanged
        {
            private string property = Guid.NewGuid().ToString();

            internal string Property
            {
                get { return this.property; }
                set { this.SetValue(ref this.property, value); }
            }
        }

        private sealed class Target : NotifyPropertyChanged
        {
            private string property = Guid.NewGuid().ToString();

            internal string Property
            {
                get { return this.property; }
                set { this.SetValue(ref this.property, value); }
            }
        }
    }
}
