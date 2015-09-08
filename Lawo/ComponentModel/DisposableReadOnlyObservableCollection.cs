////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.ComponentModel
{
    using System;
    using System.Collections.ObjectModel;

    /// <summary>Represents a disposable <see cref="ReadOnlyObservableCollection{T}"/>.</summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    public sealed class DisposableReadOnlyObservableCollection<T> : ReadOnlyObservableCollection<T>, IDisposable
    {
        private readonly Action dispose;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Calls <see cref="SubscribedObservableCollection{T}.Dispose()"/> on the
        /// <see cref="SubscribedObservableCollection{T}"/> object passed to the constructor.</summary>
        /// <remarks><see cref="SubscribedObservableCollection{T}.Dispose()"/> unsubscribes the underlying
        /// collection from change notifications. If the subscription is intended to be permanent it is permissible to
        /// never call <see cref="Dispose"/>.
        /// </remarks>
        public void Dispose()
        {
            this.dispose();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal DisposableReadOnlyObservableCollection(SubscribedObservableCollection<T> list) : base(list)
        {
            this.dispose = list.Dispose;
        }
    }
}
