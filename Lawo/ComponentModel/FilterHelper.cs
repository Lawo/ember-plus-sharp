////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.ComponentModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>Provides methods to filter and sort the items in a <see cref="ReadOnlyObservableCollection{T}"/>
    /// into a <see cref="ReadOnlyObservableCollection{T}"/>.</summary>
    /// <threadsafety static="true" instance="false"/>
    public static class FilterHelper
    {
        /// <summary>Returns an optionally sorted collection of items filtered from the items in
        /// <paramref name="originalItems"/>.</summary>
        /// <typeparam name="T">The type of the items in <paramref name="originalItems"/>.</typeparam>
        /// <param name="originalItems">The collection of original items to filter.</param>
        /// <param name="predicate">Represents the method that determines whether a given item should appear in the
        /// returned collection.</param>
        /// <param name="comparer">The <see cref="IComparer{T}"/> implementation to use to sort items;
        /// or, <c>null</c> to not sort the items.</param>
        /// <exception cref="ArgumentNullException"><paramref name="originalItems"/> and/or <paramref name="predicate"/>
        /// equal <c>null</c>.</exception>
        /// <remarks>
        /// <para>All operations on <paramref name="originalItems"/> are automatically matched by an equivalent
        /// operation on the returned collection such that an item in the original collection for which
        /// <paramref name="predicate"/> returns <c>true</c> will also appear in the returned collection.</para>
        /// </remarks>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposable object is returned to the caller.")]
        public static DisposableReadOnlyObservableCollection<T> Filter<T>(
            this ReadOnlyObservableCollection<T> originalItems, Predicate<T> predicate, IComparer<T> comparer)
        {
            return new DisposableReadOnlyObservableCollection<T>(
                new FilterCollection<T>(originalItems, predicate, comparer));
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private sealed class FilterCollection<T> : SubscribedObservableCollection<T>
        {
            private readonly Predicate<T> predicate;
            private readonly IComparer<T> comparer;

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////

            internal FilterCollection(
                ReadOnlyObservableCollection<T> originalItems, Predicate<T> predicate, IComparer<T> comparer)
            {
                if (predicate == null)
                {
                    throw new ArgumentNullException("predicate");
                }

                this.predicate = predicate;
                this.comparer = comparer;
                var handler = originalItems.AddChangeHandlers<ReadOnlyObservableCollection<T>, T>(
                    this.InsertIfMatch, this.Remove, this.Clear);
                this.RegisterForRemoval(originalItems, handler);
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////

            private void InsertIfMatch(int index, T original)
            {
                if (this.predicate(original))
                {
                    if (this.comparer == null)
                    {
                        this.Add(original);
                    }
                    else
                    {
                        int candidateIndex;

                        for (candidateIndex = 0;
                            (candidateIndex < this.Count) && (this.comparer.Compare(original, this[candidateIndex]) >= 0);
                            ++candidateIndex)
                        {
                        }

                        this.Insert(candidateIndex, original);
                    }
                }
            }

            private void Remove(int index, T original)
            {
                this.Remove(original);
            }
        }
    }
}
