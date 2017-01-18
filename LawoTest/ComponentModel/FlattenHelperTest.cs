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

    /// <summary>Tests <see cref="FlattenHelper"/>.</summary>
    [TestClass]
    public sealed class FlattenHelperTest : TestBase
    {
        /// <summary>Tests <see cref="FlattenHelper"/> by adding elements.</summary>
        [TestMethod]
        public void AddTest()
        {
            this.VerifyModification((parent, children) => parent.Add(MakeReadOnly(this.CreateRandomChild())));
            this.VerifyModification((parent, children) => this.GetRandomChild(children)?.Add(this.CreateRandomItem()));
        }

        /// <summary>Tests <see cref="FlattenHelper"/> by inserting elements.</summary>
        [TestMethod]
        public void InsertTest()
        {
            this.VerifyModification((parent, children) => parent.Insert(
                this.Random.Next(parent.Count + 1), MakeReadOnly(this.CreateRandomChild())));

            this.VerifyModification(
                (parent, children) =>
                {
                    var child = this.GetRandomChild(children);
                    child?.Insert(this.Random.Next(child.Count + 1), this.CreateRandomItem());
                });
        }

        /// <summary>Tests <see cref="FlattenHelper"/> by replacing elements.</summary>
        [TestMethod]
        public void ReplaceTest()
        {
            this.VerifyModification(
                (parent, children) =>
                {
                    if (parent.Count > 0)
                    {
                        parent[this.Random.Next(parent.Count)] = MakeReadOnly(this.CreateRandomChild());
                    }
                });

            this.VerifyModification(
                (parent, children) =>
                {
                    var child = this.GetRandomChild(children);

                    if ((child != null) && (child.Count > 0))
                    {
                        child[this.Random.Next(child.Count)] = this.CreateRandomItem();
                    }
                });
        }

        /// <summary>Tests <see cref="FlattenHelper"/> by removing elements.</summary>
        [TestMethod]
        public void RemoveTest()
        {
            this.VerifyModification(
                (parent, children) =>
                {
                    if (parent.Count > 0)
                    {
                        parent.RemoveAt(this.Random.Next(parent.Count));
                    }
                });

            this.VerifyModification(
                (parent, children) =>
                {
                    var child = this.GetRandomChild(children);

                    if ((child != null) && (child.Count > 0))
                    {
                        child.RemoveAt(this.Random.Next(child.Count));
                    }
                });
        }

        /// <summary>Tests <see cref="FlattenHelper"/> by clearing elements.</summary>
        [TestMethod]
        public void ClearTest()
        {
            this.VerifyModification((parent, children) => parent.Clear());
            this.VerifyModification((parent, children) => this.GetRandomChild(children)?.Clear());
        }

        /// <summary>Tests <see cref="FlattenHelper"/> by modifying a removed inner collection.</summary>
        [TestMethod]
        public void RemovedInnerModifyTest()
        {
            this.VerifyModification(
                (parent, children) =>
                {
                    if (children.Count > 0)
                    {
                        var randomIndex = this.Random.Next(children.Count);
                        parent.RemoveAt(randomIndex);
                        children[randomIndex]?.Add(this.CreateRandomItem());
                    }
                });
        }

        /// <summary>Tests <see cref="FlattenHelper"/> exceptions.</summary>
        [TestMethod]
        public void ExceptionTest() =>
            AssertThrow<ArgumentNullException>(
                () => ((ReadOnlyObservableCollection<ReadOnlyObservableCollection<int>>)null).Flatten());

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static ReadOnlyObservableCollection<int> MakeReadOnly(ObservableCollection<int> child) =>
            child == null ? null : new ReadOnlyObservableCollection<int>(child);

        private static List<int> GetExpected(ObservableCollection<ReadOnlyObservableCollection<int>> parent)
        {
            var expected = new List<int>();

            foreach (var child in parent)
            {
                if (child != null)
                {
                    expected.AddRange(child);
                }
            }

            return expected;
        }

        private void VerifyModification(Action<ObservableCollection<ReadOnlyObservableCollection<int>>,
            List<ObservableCollection<int>>> modification)
        {
            for (var round = 0; round < 10; ++round)
            {
                var children = Enumerable.Range(0, this.Random.Next(10)).Select(i => this.CreateRandomChild()).ToList();
                var parent = new ObservableCollection<ReadOnlyObservableCollection<int>>(children.Select(MakeReadOnly));

                using (var flattened =
                    new ReadOnlyObservableCollection<ReadOnlyObservableCollection<int>>(parent).Flatten())
                {
                    CollectionAssert.AreEqual(GetExpected(parent), flattened);
                    modification(parent, children);
                    CollectionAssert.AreEqual(GetExpected(parent), flattened);
                }
            }
        }

        private ObservableCollection<int> CreateRandomChild() =>
            this.Random.Next(5) == 0 ? null : new ObservableCollection<int>(
                Enumerable.Range(0, this.Random.Next(10)).Select(i2 => this.CreateRandomItem()));

        private int CreateRandomItem() => this.Random.Next(100);

        private T GetRandomChild<T>(IList<T> children)
            where T : class
        {
            return children.Count > 0 ? children[this.Random.Next(children.Count)] : null;
        }
    }
}
