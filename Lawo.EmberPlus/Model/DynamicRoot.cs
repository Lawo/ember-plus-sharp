////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model
{
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;

    using Lawo.EmberPlus.Ember;

    /// <summary>Represents the root containing dynamic and optional static elements in the protocol specified in the
    /// <a href="http://ember-plus.googlecode.com/svn/trunk/documentation/Ember+%20Documentation.pdf">Ember+
    /// Specification</a>.</summary>
    /// <typeparam name="TMostDerived">The most-derived subtype of this class.</typeparam>
    /// <remarks>
    /// <para>A subclass object contains all children sent by the provider. Static children can be defined and
    /// accessed as described in the <see cref="Root{T}"/> class remarks. Dynamic children are accessible through
    /// the collection exposed by <see cref="DynamicChildren"/>.</para>
    /// <para><b>Thread Safety</b>: Any public static members of this type are thread safe. Any instance members are not
    /// guaranteed to be thread safe.</para>
    /// </remarks>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance", Justification = "Fewer levels of inheritance would lead to more code duplication.")]
    public abstract class DynamicRoot<TMostDerived> : Root<TMostDerived>
        where TMostDerived : DynamicRoot<TMostDerived>
    {
        private readonly ObservableCollection<IElement> dynamicChildren = new ObservableCollection<IElement>();
        private readonly ReadOnlyObservableCollection<IElement> readOnlyDynamicChildren;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets the dynamic children of this node.</summary>
        public ReadOnlyObservableCollection<IElement> DynamicChildren
        {
            get { return this.readOnlyDynamicChildren; }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal sealed override Element ReadNewDynamicChildContents(
            EmberReader reader, ElementType actualType, Context context, out ChildrenState childChildrenState)
        {
            return DynamicNodeHelper.ReadDynamicChildContents(reader, actualType, context, out childChildrenState);
        }

        internal sealed override bool ChangeOnlineStatus(IElement child)
        {
            return DynamicNodeHelper.ChangeOnlineStatus(base.ChangeOnlineStatus, this.dynamicChildren, child);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Initializes a new instance of the <see cref="DynamicRoot{TMostDerived}"/> class.</summary>
        /// <remarks>
        /// <para>Objects of subtypes are not created by client code directly but indirectly when a
        /// <see cref="Consumer{T}"/> object is created.</para>
        /// </remarks>
        protected DynamicRoot()
        {
            this.readOnlyDynamicChildren = new ReadOnlyObservableCollection<IElement>(this.dynamicChildren);
        }
    }
}
