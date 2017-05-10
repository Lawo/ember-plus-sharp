////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.ComponentModel
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>Represents a disposable <see cref="ObservableCollection{T}"/>.</summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <threadsafety static="true" instance="false"/>
    internal abstract class SubscribedObservableCollection<T> : ObservableCollection<T>, IDisposable
    {
        /// <summary>Calls <see cref="Dispose(bool)">Dispose(true)</see>.</summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Initializes a new instance of the <see cref="SubscribedObservableCollection{T}"/> class.</summary>
        internal SubscribedObservableCollection()
        {
        }

        /// <summary>When overridden in a derived class, releases the unmanaged resources used by the
        /// <see cref="SubscribedObservableCollection{T}"/>, and optionally releases the managed resources.</summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release
        /// only unmanaged resources</param>
        /// <remarks>Removes the handler from the original collection as registered with
        /// <see cref="RegisterForRemoval"/>. Calls <see cref="IDisposable.Dispose"/>, if the original collection
        /// implements <see cref="IDisposable"/>.</remarks>
        internal virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Neither original nor handler must be null at this point. However, because Dispose must never throw, we
                // cannot throw an appropriate exception here.
                if ((this.original != null) && (this.handler != null))
                {
                    this.original.CollectionChanged -= this.handler;
                }

                (this.original as IDisposable)?.Dispose();
            }
        }

        /// <summary>Register <paramref name="theHandler"/> to be removed from the
        /// <see cref="INotifyCollectionChanged.CollectionChanged"/> event of <paramref name="theOriginal"/>.</summary>
        /// <param name="theOriginal">The original collection this collection is subscribed to.</param>
        /// <param name="theHandler">The handler to unsubscribe from the <paramref name="theOriginal"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="theOriginal"/> and/or <paramref name="theHandler"/> equal
        /// <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException"><see cref="RegisterForRemoval"/> has been called before.
        /// </exception>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", Justification = "Correct method name.")]
        internal void RegisterForRemoval(
            INotifyCollectionChanged theOriginal, NotifyCollectionChangedEventHandler theHandler)
        {
            if ((this.original != null) || (this.handler != null))
            {
                throw new InvalidOperationException("RegisterForRemoval was called more than once.");
            }

            this.original = theOriginal ?? throw new ArgumentNullException(nameof(theOriginal));
            this.handler = theHandler ?? throw new ArgumentNullException(nameof(theHandler));
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private INotifyCollectionChanged original;
        private NotifyCollectionChangedEventHandler handler;
    }
}
