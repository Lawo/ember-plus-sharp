////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.ComponentModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Lawo.UnitTesting;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Tests <see cref="FlattenHelper"/>.</summary>
    [TestClass]
    public sealed class FlattenHelperTest : TestBase
    {
        /// <summary>Tests <see cref="FlattenHelper"/> by adding elements.</summary>
        [TestMethod]
        public void AddTest()
        {
            VerifyModification((parent, children) => parent.Add(MakeReadOnly(this.CreateRandomChild())));

            VerifyModification(
                (parent, children) =>
                {
                    var child = GetRandomChild(children);

                    if (child != null)
                    {
                        child.Add(this.CreateRandomItem());
                    }
                });
        }

        /// <summary>Tests <see cref="FlattenHelper"/> by inserting elements.</summary>
        [TestMethod]
        public void InsertTest()
        {
            VerifyModification((parent, children) => parent.Insert(
                this.Random.Next(parent.Count + 1), MakeReadOnly(this.CreateRandomChild())));

            VerifyModification(
                (parent, children) =>
                {
                    var child = GetRandomChild(children);

                    if (child != null)
                    {
                        child.Insert(this.Random.Next(child.Count + 1), this.CreateRandomItem());
                    }
                });
        }

        /// <summary>Tests <see cref="FlattenHelper"/> by replacing elements.</summary>
        [TestMethod]
        public void ReplaceTest()
        {
            VerifyModification(
                (parent, children) =>
                {
                    if (parent.Count > 0)
                    {
                        parent[this.Random.Next(parent.Count)] = MakeReadOnly(this.CreateRandomChild());
                    }
                });

            VerifyModification(
                (parent, children) =>
                {
                    var child = GetRandomChild(children);

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
            VerifyModification(
                (parent, children) =>
                {
                    if (parent.Count > 0)
                    {
                        parent.RemoveAt(this.Random.Next(parent.Count));
                    }
                });

            VerifyModification(
                (parent, children) =>
                {
                    var child = GetRandomChild(children);

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
            VerifyModification((parent, children) => parent.Clear());

            VerifyModification(
                (parent, children) =>
                {
                    var child = GetRandomChild(children);

                    if (child != null)
                    {
                        child.Clear();
                    }
                });
        }

        /// <summary>Tests <see cref="FlattenHelper"/> by modifying a removed inner collection.</summary>
        [TestMethod]
        public void RemovedInnerModifyTest()
        {
            VerifyModification(
                (parent, children) =>
                {
                    if (children.Count > 0)
                    {
                        var randomIndex = this.Random.Next(children.Count);
                        parent.RemoveAt(randomIndex);
                        var child = children[randomIndex];

                        if (child != null)
                        {
                            child.Add(CreateRandomItem());
                        }
                    }
                });
        }

        /// <summary>Tests <see cref="FlattenHelper"/> exceptions.</summary>
        [TestMethod]
        public void ExceptionTest()
        {
            AssertThrow<ArgumentNullException>(
                () => ((ReadOnlyObservableCollection<ReadOnlyObservableCollection<int>>)null).Flatten());
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void VerifyModification(Action<ObservableCollection<ReadOnlyObservableCollection<int>>,
            List<ObservableCollection<int>>> modification)
        {
            for (var round = 0; round < 10; ++round)
            {
                var children = Enumerable.Range(0, this.Random.Next(10)).Select(i => CreateRandomChild()).ToList();
                var parent = new ObservableCollection<ReadOnlyObservableCollection<int>>(
                    children.Select(c => MakeReadOnly(c)));

                using (var flattened =
                    new ReadOnlyObservableCollection<ReadOnlyObservableCollection<int>>(parent).Flatten())
                {
                    CollectionAssert.AreEqual(GetExpected(parent), flattened);
                    modification(parent, children);
                    CollectionAssert.AreEqual(GetExpected(parent), flattened);
                }
            }
        }

        private ObservableCollection<int> CreateRandomChild()
        {
            return this.Random.Next(5) == 0 ? null : new ObservableCollection<int>(
                Enumerable.Range(0, this.Random.Next(10)).Select(i2 => CreateRandomItem()));
        }

        private int CreateRandomItem()
        {
            return this.Random.Next(100);
        }

        private T GetRandomChild<T>(IList<T> children) where T : class
        {
            return children.Count > 0 ? children[this.Random.Next(children.Count)] : null;
        }

        private static ReadOnlyObservableCollection<int> MakeReadOnly(ObservableCollection<int> child)
        {
            return child == null ? null : new ReadOnlyObservableCollection<int>(child);
        }

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
    }
}
