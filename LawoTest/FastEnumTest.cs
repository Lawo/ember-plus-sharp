////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using UnitTesting;

    /// <summary>Tests the <see cref="FastEnum"/> class.</summary>
    [TestClass]
    public sealed class FastEnumTest : TestBase
    {
        /// <summary>Tests <see cref="FastEnum.GetValueNameMap{TEnum}"/> and
        /// <see cref="FastEnum.GetNameValueMap{TEnum}"/>.</summary>
        [TestMethod]
        public void MapTest()
        {
            var conventionalNames = Enum.GetNames(typeof(SomeEnum)).OrderBy(n => n).ToArray();
            CollectionAssert.AreEqual(
                conventionalNames, FastEnum.GetValueNameMap<SomeEnum>().Values.OrderBy(n => n).ToArray());
            CollectionAssert.AreEqual(
                conventionalNames, FastEnum.GetNameValueMap<SomeEnum>().Keys.OrderBy(n => n).ToArray());
        }

        /// <summary>Tests the main use cases.</summary>
        [TestMethod]
        public void PerformanceTest()
        {
            PerformanceTest(IsDefinedTest, SomeEnum.Nine);
            PerformanceTest(IsDefinedTest, (SomeEnum)42);
            PerformanceTest(IsDefinedTest, SomeOtherEnum.Twelve);

            ToObjectPerformanceTest(SomeEnum.Nine);
            ToObjectPerformanceTest((SomeEnum)42);
            ToObjectPerformanceTest(SomeOtherEnum.Twelve);

            ToObjectPerformanceTest(SByteEnum.Min);
            ToObjectPerformanceTest(SByteEnum.Max);
            ToObjectPerformanceTest(ShortEnum.Min);
            ToObjectPerformanceTest(ShortEnum.Max);
            ToObjectPerformanceTest(IntEnum.Min);
            ToObjectPerformanceTest(IntEnum.Max);
            ToObjectPerformanceTest(LongEnum.Min);
            ToObjectPerformanceTest(LongEnum.Max);
            ToObjectPerformanceTest(ByteEnum.Min);
            ToObjectPerformanceTest(ByteEnum.Max);
            ToObjectPerformanceTest(UshortEnum.Min);
            ToObjectPerformanceTest(UshortEnum.Max);
            ToObjectPerformanceTest(UintEnum.Min);
            ToObjectPerformanceTest(UintEnum.Max);
            ToObjectPerformanceTest(UlongEnum.Min);
            ToObjectPerformanceTest(UlongEnum.Max);
        }

        /// <summary>Tests exceptional paths.</summary>
        [TestMethod]
        public void ExceptionTest() =>
            AssertThrow<ArgumentException>(
                () => FastEnum.IsDefined(5),
                () => FastEnum.ToEnum<int>(42),
                () => FastEnum.ToEnum<int>(42UL),
                () => FastEnum.ToInt64('G'),
                () => FastEnum.ToUInt64('G'),
                () => FastEnum.GetValueNameMap<DateTime>(),
                () => FastEnum.GetNameValueMap<char>());

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static void ToObjectPerformanceTest<T>(T value)
            where T : struct
        {
            PerformanceTest(ToObjectTest, value);
        }

        private static void PerformanceTest<T>(Func<T, int, double> test, T value)
            where T : struct
        {
            test(value, 1); // Make sure everything is JITed.
            Console.WriteLine("{0} Ratio: {1}", typeof(T).Name, test(value, 100000));
        }

        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", Justification = "Test code.")]
        private static double IsDefinedTest<T>(T value, int count)
            where T : struct
        {
            var conventionalCount = 0;
            Stopwatch conventional = new Stopwatch();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            conventional.Start();

            for (int current = 0; current < count; ++current)
            {
                conventionalCount += Enum.IsDefined(typeof(T), value) ? 1 : 0;
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            conventional.Stop();

            var fastCount = 0;
            Stopwatch fast = new Stopwatch();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            fast.Start();

            for (int current = 0; current < count; ++current)
            {
                fastCount += FastEnum.IsDefined(value) ? 1 : 0;
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            fast.Stop();
            Assert.AreEqual(conventionalCount, fastCount);

            return (double)conventional.ElapsedTicks / fast.ElapsedTicks;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", Justification = "Test code.")]
        private static double ToObjectTest<T>(T value, int count)
            where T : struct
        {
            var numericValue = FastEnum.ToInt64(value);
            Assert.AreEqual(numericValue, unchecked((long)FastEnum.ToUInt64(value)));
            Assert.AreEqual(value, FastEnum.ToEnum<T>(unchecked((ulong)numericValue)));

            var conventionalResult = default(T);
            var conventional = new Stopwatch();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            conventional.Start();

            for (int current = 0; current < count; ++current)
            {
                conventionalResult = (T)Enum.ToObject(typeof(T), numericValue);
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            conventional.Stop();

            var fastResult = default(T);
            var fast = new Stopwatch();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            fast.Start();

            for (int current = 0; current < count; ++current)
            {
                fastResult = FastEnum.ToEnum<T>(numericValue);
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            fast.Stop();
            Assert.AreEqual(value, fastResult);
            Assert.AreEqual(value, conventionalResult);

            return (double)conventional.ElapsedTicks / fast.ElapsedTicks;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private enum SomeEnum
        {
            /// <summary>One constant.</summary>
            One = 1,

            /// <summary>Two constant.</summary>
            Two,

            /// <summary>Three constant.</summary>
            Three,

            /// <summary>Four constant.</summary>
            Four,

            /// <summary>Five constant.</summary>
            Five,

            /// <summary>Six constant.</summary>
            Six,

            /// <summary>Seven constant.</summary>
            Seven,

            /// <summary>Eight constant.</summary>
            Eight,

            /// <summary>Nine constant.</summary>
            Nine,

            /// <summary>Ten constant.</summary>
            Ten
        }

        private enum SomeOtherEnum
        {
            /// <summary>Eleven constant.</summary>
            Eleven = 11,

            /// <summary>Twelve constant.</summary>
            Twelve
        }

        private enum SByteEnum : sbyte
        {
            /// <summary>Min value.</summary>
            Min = sbyte.MinValue,

            /// <summary>Max value.</summary>
            Max = sbyte.MaxValue
        }

        private enum ShortEnum : short
        {
            /// <summary>Min value.</summary>
            Min = short.MinValue,

            /// <summary>Max value.</summary>
            Max = short.MaxValue
        }

        private enum IntEnum
        {
            /// <summary>Min value.</summary>
            Min = int.MinValue,

            /// <summary>Max value.</summary>
            Max = int.MaxValue
        }

        private enum LongEnum : long
        {
            /// <summary>Min value.</summary>
            Min = long.MinValue,

            /// <summary>Max value.</summary>
            Max = long.MaxValue
        }

        private enum ByteEnum : byte
        {
            /// <summary>Min value.</summary>
            Min = byte.MinValue,

            /// <summary>Max value.</summary>
            Max = byte.MaxValue
        }

        private enum UshortEnum : ushort
        {
            /// <summary>Min value.</summary>
            Min = ushort.MinValue,

            /// <summary>Max value.</summary>
            Max = ushort.MaxValue
        }

        private enum UintEnum : uint
        {
            /// <summary>Min value.</summary>
            Min = uint.MinValue,

            /// <summary>Max value.</summary>
            Max = uint.MaxValue
        }

        private enum UlongEnum : ulong
        {
            /// <summary>Min value.</summary>
            Min = ulong.MinValue,

            /// <summary>Max value.</summary>
            Max = ulong.MaxValue
        }
    }
}
