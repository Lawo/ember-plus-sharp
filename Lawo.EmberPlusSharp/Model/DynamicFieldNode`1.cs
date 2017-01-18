////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>Represents a node containing dynamic and optional static elements in the object tree accessible through
    /// <see cref="Consumer{T}.Root">Consumer&lt;TRoot&gt;.Root</see>.</summary>
    /// <typeparam name="TMostDerived">The most-derived subtype of this class.</typeparam>
    /// <remarks>A subclass object contains all children sent by the provider. Static children can be defined and
    /// accessed as described in the <see cref="FieldNode{T}"/> class remarks. Dynamic children are accessible through
    /// the collection exposed by <see cref="DynamicChildren"/>.</remarks>
    /// <threadsafety static="true" instance="false"/>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance", Justification = "Fewer levels of inheritance would lead to more code duplication.")]
    public abstract class DynamicFieldNode<TMostDerived> : DynamicNodeBase<TMostDerived>
        where TMostDerived : DynamicFieldNode<TMostDerived>
    {
        /// <summary>Gets the dynamic children of this node.</summary>
        public ReadOnlyObservableCollection<IElement> DynamicChildren { get; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal sealed override bool ChangeVisibility(IElement child) =>
            DynamicNodeHelper.ChangeVisibility(base.ChangeVisibility, this.dynamicChildren, child);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Initializes a new instance of the <see cref="DynamicFieldNode{TMostDerived}"/> class.</summary>
        /// <remarks>
        /// <para>Objects of subtypes are not created by client code directly but indirectly when a
        /// <see cref="Consumer{T}"/> object is created.</para>
        /// </remarks>
        protected DynamicFieldNode()
        {
            this.DynamicChildren = new ReadOnlyObservableCollection<IElement>(this.dynamicChildren);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private readonly ObservableCollection<IElement> dynamicChildren = new ObservableCollection<IElement>();
    }
}
