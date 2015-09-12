////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model
{
    using System.Diagnostics.CodeAnalysis;

    using Ember;

    /// <summary>This API is not intended to be used directly from your code.</summary>
    /// <typeparam name="TMostDerived">The most-derived subtype of this class.</typeparam>
    /// <remarks>Provides the common implementation for all nodes containing dynamic elements in the object tree
    /// accessible through <see cref="Consumer{T}.Root">Consumer&lt;TRoot&gt;.Root</see>.</remarks>
    /// <threadsafety static="true" instance="false"/>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance", Justification = "Fewer levels of inheritance would lead to more code duplication.")]
    public abstract class DynamicNodeBase<TMostDerived> : FieldNode<TMostDerived>
        where TMostDerived : DynamicNodeBase<TMostDerived>
    {
        internal DynamicNodeBase()
        {
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Method is not public, CA bug?")]
        internal sealed override Element ReadNewDynamicChildContents(
            EmberReader reader, ElementType actualType, Context context, out ChildrenState childChildrenState)
        {
            return DynamicNodeHelper.ReadDynamicChildContents(reader, actualType, context, out childChildrenState);
        }
    }
}
