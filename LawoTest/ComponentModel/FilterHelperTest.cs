////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.ComponentModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using UnitTesting;

    /// <summary>Tests <see cref="FilterHelper"/>.</summary>
    [TestClass]
    public sealed class FilterHelperTest : TestBase
    {
        /// <summary>Tests <see cref="FilterHelper"/> by adding elements.</summary>
        [TestMethod]
        public void AddTest() => this.VerifyModification(original => original.Add(this.CreateRandomItem()), Predicate);

        /// <summary>Tests <see cref="FilterHelper"/> by inserting elements.</summary>
        [TestMethod]
        public void InsertTest() =>
            this.VerifyModification(
                original => original.Insert(this.Random.Next(original.Count + 1), this.CreateRandomItem()), Predicate);

        /// <summary>Tests <see cref="FilterHelper"/> by replacing elements.</summary>
        [TestMethod]
        public void ReplaceTest()
        {
            this.VerifyModification(
                original =>
                    {
                        if (original.Count > 0)
                        {
                            original[this.Random.Next(original.Count)] = this.CreateRandomItem();
                        }
                    },
                    Predicate);
        }

        /// <summary>Tests <see cref="FilterHelper"/> by removing elements.</summary>
        [TestMethod]
        public void RemoveTest()
        {
            this.VerifyModification(
                original =>
                {
                    if (original.Count > 0)
                    {
                        original.RemoveAt(this.Random.Next(original.Count));
                    }
                },
                Predicate);
        }

        /// <summary>Tests <see cref="FilterHelper"/> by clearing elements.</summary>
        [TestMethod]
        public void ClearTest() => this.VerifyModification(original => original.Clear(), Predicate);

        /// <summary>Tests <see cref="FilterHelper"/> exceptions.</summary>
        [TestMethod]
        public void ExceptionTest()
        {
            var empty = new ReadOnlyObservableCollection<int>(new ObservableCollection<int>());

            AssertThrow<ArgumentNullException>(
                () => ((ReadOnlyObservableCollection<int>)null).Filter(i => true, null),
                () => empty.Filter(null, null));
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static readonly Predicate<int> Predicate = i => i < 5;

        private static List<int> GetExpected(ObservableCollection<int> original, Predicate<int> filter, bool isSorted)
        {
            var result = original.Where(i => filter(i));

            if (isSorted)
            {
                result = result.OrderBy(i => i);
            }

            return result.ToList();
        }

        private void VerifyModification(Action<ObservableCollection<int>> modification, Predicate<int> filter)
        {
            for (var round = 0; round < 10; ++round)
            {
                var original = new ObservableCollection<int>(
                    Enumerable.Range(0, this.Random.Next(10)).Select(i => this.CreateRandomItem()));
                var isSorted = this.Random.Next(2) == 1;

                using (var filtered = new ReadOnlyObservableCollection<int>(original).Filter(
                    filter, isSorted ? Comparer<int>.Default : null))
                {
                    CollectionAssert.AreEqual(GetExpected(original, filter, isSorted), filtered);
                    modification(original);
                    var expected = GetExpected(original, filter, isSorted);

                    if (isSorted)
                    {
                        CollectionAssert.AreEqual(expected, filtered);
                    }
                    else
                    {
                        CollectionAssert.AreEquivalent(expected, filtered);
                    }
                }
            }
        }

        private int CreateRandomItem() => this.Random.Next(10);
    }
}
