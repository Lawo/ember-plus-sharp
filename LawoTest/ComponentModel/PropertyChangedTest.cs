////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2016 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.ComponentModel
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Lawo.Reflection;
    using Lawo.UnitTesting;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Tests <see cref="PropertyChangedRegistration"/> and <see cref="NotifyPropertyChanged"/>.</summary>
    [TestClass]
    public sealed class PropertyChangedTest : TestBase
    {
        /// <summary>Tests the main <see cref="PropertyChangedRegistration"/> use cases.</summary>
        [TestMethod]
        public void PropertyChangedRegistrationTest()
        {
            var addends = new[] { new Addend(), new Addend() };
            Func<PropertyChangedEventHandler, IDisposable> createCalculated =
                handler => new PropertyChangedRegistration(
                    handler, addends.Select(a => a.GetProperty(o => o.AddendValue)).ToArray());
            PropertyChangedEventHandler validateHandler =
                (s, e) =>
                {
                    Assert.IsTrue(Array.IndexOf(addends, s) >= 0);
                    Assert.AreEqual("AddendValue", e.PropertyName);
                };

            this.TestCore(createCalculated, false, validateHandler, addends);
        }

        /// <summary>Tests the exceptional <see cref="PropertyChangedRegistration"/> use cases.</summary>
        [TestMethod]
        public void ExceptionTest()
        {
            AssertThrow<ArgumentNullException>(
                () => new PropertyChangedRegistration(null, new Addend().GetProperty(o => o.AddendValue)).Dispose(),
                () => new PropertyChangedRegistration((s, e) => { }, null).Dispose());
            AssertThrow<ArgumentException>(
                () => new PropertyChangedRegistration((s, e) => { }, NullProperty).Dispose());
            new Exceptional().ToString();
        }

        /// <summary>Tests <see cref="NotifyPropertyChanged"/>.</summary>
        [TestMethod]
        public void NotifyPropertyChangedTest()
        {
            new CalculatedSum(this, 1).ToString();
            new CalculatedSum(this, 2).ToString();
            new CalculatedSum(this, 3).ToString();
            new CalculatedSum(this, 4).ToString();
            new CalculatedSum(this, 5).ToString();
            new CalculatedSum(this, 6).ToString();
            new CalculatedSum(this, 7).ToString();
            new CalculatedSum(this, 8).ToString();
            new CalculatedSum(this, 9).ToString();
            new CalculatedSum(this, 10).ToString();
            new CalculatedSum(this, 11).ToString();
        }

        /// <summary>Exposes a NullReferenceException bug, see change history for details.</summary>
        [TestMethod]
        public void NullReferenceExceptionBugTest()
        {
            var source = new Addend(1);
            var target = new NullReferenceExceptionBug();
            CalculatedProperty.Create(
                source.GetProperty(o => o.AddendValue), v => v, target.GetProperty(o => o.Value)).Dispose();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private const IProperty<INotifyPropertyChanged> NullProperty = null;

        private void TestCore(
            Func<PropertyChangedEventHandler, IDisposable> createCalculated,
            bool creationIsNotified,
            PropertyChangedEventHandler validate,
            params Addend[] addends)
        {
            int totalCount;
            var changedCount = 0;

            using (var calculated = createCalculated(validate + ((s, e) => ++changedCount)))
            {
                totalCount = this.IncrementAddends(0, addends) + (creationIsNotified ? 1 : 0);
                Assert.AreEqual(totalCount, changedCount);
            }

            foreach (var addend in addends)
            {
                addend.AddendValue = 0;
            }

            Assert.AreEqual(totalCount, changedCount);
        }

        private int IncrementAddends(int index, params Addend[] addends)
        {
            var totalCount = 0;

            if (index < addends.Length)
            {
                // The min value for count must be 2 so that there is a guarantee that AddedValue is changed for every loop
                // revolution.
                var count = this.Random.Next(2, 5);

                for (var value = 1; value <= count; ++value)
                {
                    addends[index].AddendValue = value;
                    totalCount += this.IncrementAddends(index + 1, addends) + 1;
                }
            }

            return totalCount;
        }

        private sealed class Addend : NotifyPropertyChanged
        {
            internal Addend()
            {
            }

            internal Addend(int value)
            {
                this.addendValue = value;
            }

            internal int AddendValue
            {
                get { return this.addendValue; }
                set { this.SetValue(ref this.addendValue, value); }
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////

            private int addendValue;
        }

        private sealed class CalculatedSum : NotifyPropertyChanged
        {
            internal CalculatedSum(PropertyChangedTest test, int propertyCount)
            {
                this.addends = Enumerable.Range(1, propertyCount).Select(i => new Addend()).ToArray();
                this.sum = CreateSum(this.GetProperty(o => o.SumValue), this.addends);

                Func<PropertyChangedEventHandler, IDisposable> createCalculated =
                    handler =>
                    {
                        this.PropertyChanged += handler;
                        return this.sum;
                    };

                PropertyChangedEventHandler validateHandler =
                    (s, e) =>
                    {
                        Assert.AreEqual(this, s);
                        Assert.AreEqual("SumValue", e.PropertyName);
                        Assert.AreEqual(this.addends.Aggregate(0, (sum, addend) => sum + addend.AddendValue), this.SumValue);
                    };

                test.TestCore(createCalculated, false, validateHandler, this.addends);
            }

            internal int SumValue
            {
                get { return this.sum.Value; }
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////

            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Test code.")]
            private static CalculatedProperty<int> CreateSum(
                IProperty<NotifyPropertyChanged, int> calculated, params Addend[] addends)
            {
                var ps = addends.Select(a => a.GetProperty(o => o.AddendValue)).ToArray();

                switch (ps.Length)
                {
                    case 1:
                        return CalculatedProperty.Create(ps[0], calculated);
                    case 2:
                        return CalculatedProperty.Create(ps[0], ps[1], (p1, p2) => p1 + p2, calculated);
                    case 3:
                        return CalculatedProperty.Create(ps[0], ps[1], ps[2], (p1, p2, p3) => p1 + p2 + p3, calculated);
                    case 4:
                        return CalculatedProperty.Create(
                            ps[0], ps[1], ps[2], ps[3], (p1, p2, p3, p4) => p1 + p2 + p3 + p4, calculated);
                    case 5:
                        return CalculatedProperty.Create(
                            ps[0],
                            ps[1],
                            ps[2],
                            ps[3],
                            ps[4],
                            (p1, p2, p3, p4, p5) => p1 + p2 + p3 + p4 + p5,
                            calculated);
                    case 6:
                        return CalculatedProperty.Create(
                            ps[0],
                            ps[1],
                            ps[2],
                            ps[3],
                            ps[4],
                            ps[5],
                            (p1, p2, p3, p4, p5, p6) => p1 + p2 + p3 + p4 + p5 + p6,
                            calculated);
                    case 7:
                        return CalculatedProperty.Create(
                            ps[0],
                            ps[1],
                            ps[2],
                            ps[3],
                            ps[4],
                            ps[5],
                            ps[6],
                            (p1, p2, p3, p4, p5, p6, p7) => p1 + p2 + p3 + p4 + p5 + p6 + p7,
                            calculated);
                    case 8:
                        return CalculatedProperty.Create(
                            ps[0],
                            ps[1],
                            ps[2],
                            ps[3],
                            ps[4],
                            ps[5],
                            ps[6],
                            ps[7],
                            (p1, p2, p3, p4, p5, p6, p7, p8) => p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8,
                            calculated);
                    case 9:
                        return CalculatedProperty.Create(
                            ps[0],
                            ps[1],
                            ps[2],
                            ps[3],
                            ps[4],
                            ps[5],
                            ps[6],
                            ps[7],
                            ps[8],
                            (p1, p2, p3, p4, p5, p6, p7, p8, p9) => p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9,
                            calculated);
                    case 10:
                        return CalculatedProperty.Create(
                            ps[0],
                            ps[1],
                            ps[2],
                            ps[3],
                            ps[4],
                            ps[5],
                            ps[6],
                            ps[7],
                            ps[8],
                            ps[9],
                            (p1, p2, p3, p4, p5, p6, p7, p8, p9, p10) => p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9 + p10,
                            calculated);
                    default:
                        return CalculatedProperty.Create(ps, vs => vs.Aggregate((a, r) => a + r), calculated);
                }
            }

            private readonly Addend[] addends;
            private readonly CalculatedProperty<int> sum;
        }

        private sealed class Exceptional : NotifyPropertyChanged
        {
            internal Exceptional()
            {
                AssertThrow<ArgumentNullException>(
                    () => CalculatedProperty.Create((IProperty<NotifyPropertyChanged, int>)null, v => v, this.GetProperty(o => o.Calculated)).Dispose(),
                    () => CalculatedProperty.Create(this.GetProperty(o => o.Value), null, this.GetProperty(o => o.Calculated)).Dispose(),
                    () => CalculatedProperty.Create<int, int>(this.GetProperty(o => o.Value), v => v, null).Dispose());

                AssertThrow<ArgumentNullException>(() => MultiBinding.Create(this.GetProperty(o => o.Value), null).Dispose());
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////

            private int valueField;

            private int Calculated
            {
                get { return this.Value; }
            }

            private int Value
            {
                get { return this.valueField; }
                set { this.SetValue(ref this.valueField, value); }
            }
        }

        private sealed class NullReferenceExceptionBug : NotifyPropertyChanged
        {
            public int Value
            {
                get { return this.theValue; }
                set { this.SetValue(ref this.theValue, value); }
            }

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

            private int theValue;
        }
    }
}
