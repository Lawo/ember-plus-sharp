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

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using UnitTesting;

    /// <summary>Tests the <see cref="GenericCompare"/> class.</summary>
    [TestClass]
    public sealed class GenericCompareTest : TestBase
    {
        /// <summary>Tests the main use cases.</summary>
        [TestMethod]
        public void MainTest()
        {
            Test(3, 5);
            Test(4, 4);
            Test(4UL, 7UL);
            Test(DateTime.Now, DateTime.Now);
            Test(GetRandomString(), GetRandomString());
            var random1 = GetRandomString() + GetRandomString() + GetRandomString() + GetRandomString();
            Test(random1 + "X", random1 + "X");
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static void Test<T>(T first, T second)
        {
            TestCore(first, second, 1); // Make sure everything is JITed.
            Console.WriteLine("{0} Ratio: {1}", typeof(T).Name, TestCore(first, second, 1000000));
        }

        private static double TestCore<T>(T first, T second, int count)
        {
            var conventionalCount = 0;
            Stopwatch convetional = new Stopwatch();
            convetional.Start();

            for (int current = 0; current < count; ++current)
            {
                conventionalCount += Equals(first, second) ? 1 : 0;
            }

            convetional.Stop();

            var genericCount = 0;
            Stopwatch generic = new Stopwatch();
            generic.Start();

            for (int current = 0; current < count; ++current)
            {
                genericCount += GenericCompare.Equals(first, second) ? 1 : 0;
            }

            generic.Stop();
            Assert.AreEqual(conventionalCount, genericCount);

            return (double)convetional.ElapsedTicks / generic.ElapsedTicks;
        }
    }
}
