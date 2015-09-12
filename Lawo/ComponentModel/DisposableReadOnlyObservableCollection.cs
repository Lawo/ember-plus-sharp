////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.ComponentModel
{
    using System;
    using System.Collections.ObjectModel;

    /// <summary>Represents a disposable <see cref="ReadOnlyObservableCollection{T}"/>.</summary>
    /// <typeparam name="T">The type of the elements in the collection.</typeparam>
    /// <threadsafety static="true" instance="false"/>
    public sealed class DisposableReadOnlyObservableCollection<T> : ReadOnlyObservableCollection<T>, IDisposable
    {
        private readonly Action dispose;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Unsubscribes the underlying collection from change notifications.</summary>
        /// <remarks>If the subscription is intended to be permanent it is permissible to never call
        /// <see cref="Dispose"/>.</remarks>
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
