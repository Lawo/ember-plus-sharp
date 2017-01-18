////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.ComponentModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using UnitTesting;

    /// <summary>Tests <see cref="ObservableCollectionHelper"/>.</summary>
    [TestClass]
    public sealed class ObservableCollectionHelperTest : TestBase
    {
        /// <summary>Tests the main <see cref="ObservableCollectionHelper"/> use cases.</summary>
        [TestMethod]
        public void MainTest()
        {
            this.AssertChange(c => { });
            this.AssertChange(c => c.Add(42));
            this.AssertChange(c => c.Insert(1, 42));
            this.AssertChange(c => c.Remove(2));
            this.AssertChange(c => c.RemoveAt(3));
            this.AssertChange(c => c.Move(1, 3));
            this.AssertChange(c => c[2] = 42);
            this.AssertChange(c => c.Clear());
        }

        /// <summary>Tests the exceptions thrown by <see cref="ObservableCollectionHelper"/>.</summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Test code.")]
        [TestMethod]
        public void ExceptionTest()
        {
            var collection = new ObservableCollection<int>();
            AssertThrow<ArgumentNullException>(
                () => ((ObservableCollection<int>)null).AddChangeHandlers((int i, int j) => { }, (i, j) => { }, () => { }),
                () => collection.AddChangeHandlers(null, (int i, int j) => { }, () => { }),
                () => collection.AddChangeHandlers((int i, int j) => { }, null, () => { }),
                () => collection.AddChangeHandlers((int i, int j) => { }, (i, j) => { }, null),
                () => ((ObservableCollection<int>)null).Project((int i) => i),
                () => collection.Project((Func<int, int>)null));
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static void AssertChange(
            ObservableCollection<int> original,
            Action<ObservableCollection<int>> change,
            ICollection copy,
            NotifyCollectionChangedEventHandler handler)
        {
            try
            {
                change(original);
                CollectionAssert.AreEqual(original, copy);
            }
            finally
            {
                original.CollectionChanged -= handler;
            }

            original.Add(42);
            CollectionAssert.AreNotEqual(original, copy);
        }

        private static void AssertChange(
            ObservableCollection<int> original, Action<ObservableCollection<int>> change, ICollection copy)
        {
            change(original);
            CollectionAssert.AreEqual(original, copy);
        }

        private void AssertChange(Action<ObservableCollection<int>> change)
        {
            var o = new ObservableCollection<int>(Enumerable.Range(0, this.Random.Next(4, 10)));
            var c = new List<int>();
            var handler = o.AddChangeHandlers(
                (int index, int item) => c.Insert(index, item), (index, item) => c.RemoveAt(index), c.Clear);
            AssertChange(o, change, c, handler);

            using (var projection = o.Project((int i) => i))
            {
                AssertChange(o, change, projection);
            }
        }
    }
}
