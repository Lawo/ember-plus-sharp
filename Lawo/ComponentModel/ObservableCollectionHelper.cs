////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.ComponentModel
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;

    /// <summary>Provides helper methods for collections that implement <see cref="INotifyCollectionChanged"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public static class ObservableCollectionHelper
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Adds a handler to <paramref name="collection"/> that translates collection changes and forwards
        /// them to <paramref name="added"/>, <paramref name="removed"/> and <paramref name="cleared"/> as appropriate.
        /// </summary>
        /// <typeparam name="TCollection">The type of the collection.</typeparam>
        /// <typeparam name="TItem">The type of the items in the collection.</typeparam>
        /// <param name="collection">The collection to add the handler to.</param>
        /// <param name="added">The action to execute when an item is added to the collection. The parameters are the
        /// index at which the item was added, followed by the item.</param>
        /// <param name="removed">The action to execute when an item is removed from the collection. The parameters are
        /// the index at which the item was removed, followed by the item.</param>
        /// <param name="cleared">The action to execute when all items have been removed from the collection.</param>
        /// <returns>The added handler. To stop calls to <paramref name="added"/>, <paramref name="removed"/> and
        /// <paramref name="cleared"/>, this handler needs to be removed from
        /// <see cref="INotifyCollectionChanged.CollectionChanged"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/>, <paramref name="added"/>,
        /// <paramref name="removed"/> and/or <paramref name="cleared"/> equal <c>null</c>.</exception>
        /// <remarks>
        /// <para>First calls <paramref name="added"/> for each item <paramref name="collection"/> currently
        /// contains, then adds the handler to <see cref="INotifyCollectionChanged.CollectionChanged"/> and finally
        /// returns the added handler.</para>
        /// </remarks>
        public static NotifyCollectionChangedEventHandler AddChangeHandlers<TCollection, TItem>(
            this TCollection collection, Action<int, TItem> added, Action<int, TItem> removed, Action cleared)
            where TCollection : IList, INotifyCollectionChanged
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (added == null)
            {
                throw new ArgumentNullException(nameof(added));
            }

            if (removed == null)
            {
                throw new ArgumentNullException(nameof(removed));
            }

            if (cleared == null)
            {
                throw new ArgumentNullException(nameof(cleared));
            }

            Add(0, collection, added);
            NotifyCollectionChangedEventHandler handler = (s, e) => OnCollectionChanged(s, e, added, removed, cleared);
            collection.CollectionChanged += handler;
            return handler;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static void OnCollectionChanged<T>(
            object sender, NotifyCollectionChangedEventArgs e, Action<int, T> added, Action<int, T> removed, Action cleared)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                cleared();
                Add(0, (IList)sender, added);
            }
            else
            {
                if (e.OldItems != null)
                {
                    for (var index = 0; index < e.OldItems.Count; ++index)
                    {
                        removed(e.OldStartingIndex + index, (T)e.OldItems[index]);
                    }
                }

                Add(e.NewStartingIndex, e.NewItems, added);
            }
        }

        private static void Add<T>(int startIndex, IList items, Action<int, T> added)
        {
            if (items != null)
            {
                for (var index = 0; index < items.Count; ++index)
                {
                    added(startIndex + index, (T)items[index]);
                }
            }
        }
    }
}
