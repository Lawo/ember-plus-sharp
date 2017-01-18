////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>Represents a node containing an unbounded number of identically-typed elements in the object tree
    /// accessible through <see cref="Consumer{T}.Root">Consumer&lt;TRoot&gt;.Root</see>.</summary>
    /// <typeparam name="TElement">The type of the elements this node contains. This must either be an
    /// <see cref="Element{T}"/> subclass, <see cref="IParameter"/>, <see cref="INode"/> or <see cref="IFunction"/>.
    /// </typeparam>
    /// <threadsafety static="true" instance="false"/>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance", Justification = "Fewer levels of inheritance would lead to more code duplication.")]
    public sealed class CollectionNode<TElement> : CollectionNode<CollectionNode<TElement>, TElement>
        where TElement : IElement
    {
        private CollectionNode()
        {
        }
    }
}
