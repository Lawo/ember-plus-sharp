////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>Provides the common interface for all nodes in the object tree accessible through
    /// <see cref="Consumer{T}.Root">Consumer&lt;TRoot&gt;.Root</see>.</summary>
    public interface INode : IElementWithSchemas
    {
        /// <summary>Gets a value indicating whether this is a root node.</summary>
        bool IsRoot { get; }

        /// <summary>Gets or sets the policy for this node.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Attempted to set a value that is not equal to one of the named
        /// constants of <see cref="ChildrenRetrievalPolicy"/>.</exception>
        /// <exception cref="InvalidOperationException">Attempted to set a new value when the current value is not equal to
        /// <see cref="Model.ChildrenRetrievalPolicy.None"/>.</exception>
        /// <remarks>Setting this property prompts the consumer to retrieve direct and indirect children according to
        /// the new value. The retrieval starts automatically when
        /// <see cref="Consumer{TRoot}.AutoSendInterval">Consumer&lt;TRoot&gt;.AutoSendInterval</see> elapses. To
        /// explicitly wait for the children to be retrieved, <see langword="await"/> the result of a call to
        /// <see cref="Consumer{TRoot}.SendAsync">Consumer&lt;TRoot&gt;.SendAsync</see>.</remarks>
        ChildrenRetrievalPolicy ChildrenRetrievalPolicy { get; set; }

        /// <summary>Gets the children of this node.</summary>
        ReadOnlyObservableCollection<IElement> Children { get; }

        /// <summary>Gets the child where <see cref="IElement.Number"/> equals <paramref name="number"/>.</summary>
        /// <exception cref="KeyNotFoundException">No child exists where <see cref="IElement.Number"/> equals
        /// <paramref name="number"/>.</exception>
        IElement this[int number] { get; }

        /// <summary>Gets the element with the path <paramref name="path"/> relative to this node.</summary>
        /// <param name="path">The relative path to the desired element.</param>
        /// <returns>The element with the given path, if such an element exists; otherwise, <c>null</c>.</returns>
        IElement GetElement(string path);
    }
}
