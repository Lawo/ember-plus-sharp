////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Threading;

    using Ember;

    /// <summary>Represents a node containing a number of static elements in the object tree accessible through
    /// <see cref="Consumer{T}.Root">Consumer&lt;TRoot&gt;.Root</see>.</summary>
    /// <typeparam name="TMostDerived">The most-derived subtype of this class.</typeparam>
    /// <remarks>
    /// <para><typeparamref name="TMostDerived"/> must contain a property with a getter and a setter for each
    /// child of the represented node. The property getters and setters can have any accessibility. The name of each
    /// property must be equal to the identifier of the corresponding child, or carry an
    /// <see cref="ElementAttribute"/> to which the identifier is passed.</para>
    /// <para>The type of each <typeparamref name="TMostDerived"/> property must be of one of the following:
    /// <list type="bullet">
    /// <item><see cref="IParameter"/>.</item>
    /// <item><see cref="INode"/>.</item>
    /// <item><see cref="IFunction"/>.</item>
    /// <item><see cref="IMatrix"/>.</item>
    /// <item><see cref="BooleanParameter"/>.</item>
    /// <item><see cref="EnumParameter{TEnum}"/>.</item>
    /// <item><see cref="IntegerParameter"/>.</item>
    /// <item><see cref="OctetstringParameter"/>.</item>
    /// <item><see cref="RealParameter"/>.</item>
    /// <item><see cref="StringParameter"/>.</item>
    /// <item><see cref="NullableBooleanParameter"/>.</item>
    /// <item><see cref="NullableEnumParameter{TEnum}"/>.</item>
    /// <item><see cref="NullableIntegerParameter"/>.</item>
    /// <item><see cref="NullableOctetstringParameter"/>.</item>
    /// <item><see cref="NullableRealParameter"/>.</item>
    /// <item><see cref="NullableStringParameter"/>.</item>
    /// <item><see cref="CollectionNode{TElement}"/>.</item>
    /// <item>A <see cref="FieldNode{TMostDerived}"/> subtype.</item>
    /// <item>A <see cref="DynamicFieldNode{TMostDerived}"/> subtype.</item>
    /// <item>A <see cref="Matrix{TMostDerived}"/> subtype.</item>
    /// <item>A <see cref="DynamicMatrix{TMostDerived}"/> subtype.</item>
    /// <item><see cref="Function{T}"/>, <see cref="Function{T, U}"/>, <see cref="Function{T, U, V}"/>,
    /// <see cref="Function{T, U, V, W}"/>, <see cref="Function{T, U, V, W, X}"/>,
    /// <see cref="Function{T, U, V, W, X, Y}"/>, or <see cref="Function{T, U, V, W, X, Y, Z}"/>.</item>
    /// </list></para>
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance", Justification = "Fewer levels of inheritance would lead to more code duplication.")]
    public abstract partial class FieldNode<TMostDerived> : Node<TMostDerived>
        where TMostDerived : FieldNode<TMostDerived>
    {
        internal virtual Element ReadNewDynamicChildContents(
            EmberReader reader, ElementType actualType, Context context, out RetrievalState childRetrievalState)
        {
            return base.ReadNewChildContents(reader, actualType, context, out childRetrievalState);
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Method is not public, CA bug?")]
        internal override bool ChangeVisibility(IElement child)
        {
            base.ChangeVisibility(child);
            MetaElement metaChild;
            var result = MetaChildren.TryGetValue(child.Identifier, out metaChild);

            if (result)
            {
                metaChild.ChangeVisibility(this, child);
            }

            return result;
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Method is not public, CA bug?")]
        internal sealed override Element ReadNewChildContents(
            EmberReader reader, ElementType actualType, Context context, out RetrievalState childRetrievalState)
        {
            MetaElement metaChild;
            return MetaChildren.TryGetValue(context.Identifier, out metaChild) ?
                metaChild.ReadContents(reader, actualType, context, out childRetrievalState) :
                this.ReadNewDynamicChildContents(reader, actualType, context, out childRetrievalState);
        }

        internal sealed override bool AreRequiredChildrenAvailable(bool throwIfMissing)
        {
            if (!this.IsOnline)
            {
                return true;
            }

            foreach (var metaChild in MetaChildren.Values)
            {
                if (!metaChild.IsAvailable(this, throwIfMissing))
                {
                    return false;
                }
            }

            return true;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Initializes a new instance of the <see cref="FieldNode{TMostDerived}"/> class.</summary>
        /// <remarks>
        /// <para>Objects of subtypes are not created by client code directly but indirectly when a
        /// <see cref="Consumer{T}"/> object is created.</para>
        /// </remarks>
        protected FieldNode()
        {
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static Dictionary<string, MetaElement> metaChildren;

        private static Dictionary<string, MetaElement> MetaChildren =>
            LazyInitializer.EnsureInitialized(ref metaChildren, GetMetaChildren);

        private static Dictionary<string, MetaElement> GetMetaChildren()
        {
            try
            {
                return typeof(TMostDerived).GetTypeInfo().DeclaredProperties.Select(MetaElement.Create).ToDictionary(
                    e => e.Identifier);
            }
            catch (ArgumentException ex)
            {
                const string Format = "Duplicate identifier found in {0}.";
                throw new ModelException(string.Format(CultureInfo.InvariantCulture, Format, typeof(TMostDerived)), ex);
            }
        }
    }
}
