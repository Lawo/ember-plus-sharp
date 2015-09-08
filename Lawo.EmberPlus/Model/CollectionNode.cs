////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using Lawo.EmberPlus.Ember;

    /// <summary>Represents a node containing a unbounded number of identically-typed elements in the protocol specified
    /// in the
    /// <a href="http://ember-plus.googlecode.com/svn/trunk/documentation/Ember+%20Documentation.pdf">Ember+
    /// Specification</a>.</summary>
    /// <typeparam name="TElement">The type of the elements this node contains. This must either be an
    /// <see cref="Element{T}"/> subclass, <see cref="IParameter"/>, <see cref="INode"/> or <see cref="IFunction"/>.
    /// </typeparam>
    /// <remarks>
    /// <para><b>Thread Safety</b>: Any public static members of this type are thread safe. Any instance members are not
    /// guaranteed to be thread safe.</para>
    /// </remarks>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance", Justification = "Fewer levels of inheritance would lead to more code duplication.")]
    public sealed class CollectionNode<TElement> : Node<CollectionNode<TElement>> where TElement : IElement
    {
        private static readonly ReadContentsMethod ReadContentsCallback = GetReadContentsMethod();
        private readonly ObservableCollection<TElement> children = new ObservableCollection<TElement>();
        private readonly ReadOnlyObservableCollection<TElement> readOnlyChildren;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets the children of this node.</summary>
        public ReadOnlyObservableCollection<TElement> Children
        {
            get { return this.readOnlyChildren; }
        }

        /// <summary>Gets the child where <see cref="Element.Number"/> equals <paramref name="number"/>.</summary>
        /// <exception cref="KeyNotFoundException">No child exists where <see cref="Element.Number"/> equals
        /// <paramref name="number"/>.</exception>
        public TElement this[int number]
        {
            get { return (TElement)this.GetChild(number); }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", Justification = "These are actual class names.")]
        internal sealed override Element ReadNewChildContents(
            EmberReader reader, ElementType actualType, Context context, out ChildrenState childChildrenState)
        {
            if (ReadContentsCallback == null)
            {
                const string Format = "The type argument passed to CollectionNode<TElement> with the path {0} is neither " +
                    "an Element<TMostDerived> subclass nor IParameter nor INode nor IFunction.";
                throw new ModelException(string.Format(CultureInfo.InvariantCulture, Format, this.GetPath()));
            }

            return ReadContentsCallback(reader, actualType, context, out childChildrenState);
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Method is not public, CA bug?")]
        internal sealed override bool ChangeOnlineStatus(IElement child)
        {
            base.ChangeOnlineStatus(child);

            if (child.IsOnline)
            {
                this.children.Add((TElement)child);
            }
            else
            {
                this.children.Remove((TElement)child);
            }

            return true;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private CollectionNode()
        {
            this.readOnlyChildren = new ReadOnlyObservableCollection<TElement>(this.children);
        }

        private static ReadContentsMethod GetReadContentsMethod()
        {
            var implementationType = Element.GetImplementationType(typeof(TElement));
            return (implementationType == null) ? null :
                (ReadContentsMethod)typeof(Element<>).MakeGenericType(implementationType).GetRuntimeMethods().First(
                    m => m.Name == "ReadContents").CreateDelegate(typeof(ReadContentsMethod));
        }

        private delegate Element ReadContentsMethod(
            EmberReader reader, ElementType actualType, Context context, out ChildrenState childChildrenState);
    }
}
