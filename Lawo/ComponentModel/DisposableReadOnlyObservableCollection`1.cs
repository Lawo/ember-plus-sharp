////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
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
        /// <summary>Unsubscribes the underlying collection from change notifications.</summary>
        /// <remarks>If the subscription is intended to be permanent it is permissible to never call
        /// <see cref="Dispose"/>.</remarks>
        public void Dispose() => this.dispose();

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal DisposableReadOnlyObservableCollection(SubscribedObservableCollection<T> list)
            : base(list)
        {
            this.dispose = list.Dispose;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private readonly Action dispose;
    }
}
