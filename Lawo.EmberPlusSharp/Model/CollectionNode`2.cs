////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;

    using Ember;

    /// <summary>Represents a node containing an unbounded number of mostly identically-typed elements in the object
    /// tree accessible through <see cref="Consumer{T}.Root">Consumer&lt;TRoot&gt;.Root</see>.</summary>
    /// <typeparam name="TMostDerived">The most-derived subtype of this class.</typeparam>
    /// <typeparam name="TElement">The type of the elements this node contains. This must either be an
    /// <see cref="Element{T}"/> subclass, <see cref="IParameter"/>, <see cref="INode"/> or <see cref="IFunction"/>.
    /// </typeparam>
    /// <remarks>In contrast to <see cref="CollectionNode{TElement}"/>, which can only contain elements of the same
    /// type, this class additionally offers the features of <see cref="FieldNode{TMostDerived}"/>. This is useful to
    /// model nodes containing primarily elements of the same type with a few other elements added. In practice,
    /// collection nodes e.g. often also contain functions to add and delete elements.</remarks>
    /// <threadsafety static="true" instance="false"/>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance", Justification = "Fewer levels of inheritance would lead to more code duplication.")]
    public abstract class CollectionNode<TMostDerived, TElement> : FieldNode<TMostDerived>
        where TMostDerived : CollectionNode<TMostDerived, TElement>
        where TElement : IElement
    {
        /// <summary>Gets the children of this node.</summary>
        public ReadOnlyObservableCollection<TElement> Children { get; }

        /// <summary>Gets the child where <see cref="Element.Number"/> equals <paramref name="number"/>.</summary>
        /// <exception cref="KeyNotFoundException">No child exists where <see cref="Element.Number"/> equals
        /// <paramref name="number"/>.</exception>
        public TElement this[int number] => (TElement)this.GetChild(number);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", Justification = "These are actual class names.")]
        internal sealed override Element ReadNewDynamicChildContents(
            EmberReader reader, ElementType actualType, Context context, out RetrievalState childRetrievalState)
        {
            if (ReadContentsCallback == null)
            {
                const string Format = "The type argument passed to CollectionNode<TElement> with the path {0} is neither " +
                    "an Element<TMostDerived> subclass nor IParameter nor INode nor IFunction.";
                throw new ModelException(string.Format(CultureInfo.InvariantCulture, Format, this.GetPath()));
            }

            return ReadContentsCallback(reader, actualType, context, out childRetrievalState);
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Method is not public, CA bug?")]
        internal sealed override bool ChangeVisibility(IElement child)
        {
            if (!base.ChangeVisibility(child))
            {
                VisibilityHelper.ChangeVisibility(this.children, (TElement)child);
            }

            return true;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Initializes a new instance of the <see cref="CollectionNode{TMostDerived, TElement}"/> class.
        /// </summary>
        protected CollectionNode()
        {
            this.Children = new ReadOnlyObservableCollection<TElement>(this.children);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static readonly ReadContentsMethod ReadContentsCallback = GetReadContentsMethod();

        private static ReadContentsMethod GetReadContentsMethod()
        {
            var implementationType = GetImplementationType(typeof(TElement));
            return implementationType == null ? null :
                (ReadContentsMethod)typeof(Element<>).MakeGenericType(implementationType).GetRuntimeMethods().First(
                    m => m.Name == "ReadContents").CreateDelegate(typeof(ReadContentsMethod));
        }

        private readonly ObservableCollection<TElement> children = new ObservableCollection<TElement>();

        private delegate Element ReadContentsMethod(
            EmberReader reader, ElementType actualType, Context context, out RetrievalState childRetrievalState);
    }
}
