////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model
{
    using System.Diagnostics.CodeAnalysis;

    using Ember;

    /// <summary>Represents the common base of nodes containing dynamic elements in the protocol specified in the
    /// <a href="http://ember-plus.googlecode.com/svn/trunk/documentation/Ember+%20Documentation.pdf">Ember+
    /// Specification</a>.</summary>
    /// <typeparam name="TMostDerived">The most-derived subtype of this class.</typeparam>
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
