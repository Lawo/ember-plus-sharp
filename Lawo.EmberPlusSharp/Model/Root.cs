////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;

    using Ember;
    using Glow;

    /// <summary>Represents a root object containing a number of static elements accessible through
    /// <see cref="Consumer{T}.Root">Consumer&lt;TRoot&gt;.Root</see>.</summary>
    /// <typeparam name="TMostDerived">The most-derived subtype of this class.</typeparam>
    /// <remarks>
    /// <para><typeparamref name="TMostDerived"/> must contain a property with a getter and a setter for each root
    /// element. The property getters and setters can have any accessibility. The name of each property must be equal to
    /// the identifier of the corresponding element, or carry an <see cref="ElementAttribute"/> to which the identifier
    /// is passed.</para>
    /// <para>The type of each <typeparamref name="TMostDerived"/> property must be of one of the following:
    /// <list type="bullet">
    /// <item><see cref="BooleanParameter"/>.</item>
    /// <item><see cref="IntegerParameter"/>.</item>
    /// <item><see cref="OctetstringParameter"/>.</item>
    /// <item><see cref="RealParameter"/>.</item>
    /// <item><see cref="StringParameter"/>.</item>
    /// <item>A <see cref="FieldNode{TMostDerived}"/> subtype.</item>
    /// <item><see cref="CollectionNode{TElement}"/>.</item>
    /// </list></para>
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance", Justification = "Fewer levels of inheritance would lead to more code duplication.")]
    public abstract class Root<TMostDerived> : FieldNode<TMostDerived>, IParent where TMostDerived : Root<TMostDerived>
    {
        void IParent.SetHasChanges()
        {
            if (!this.HasChanges)
            {
                this.HasChanges = true;

                var handler = this.HasChangesSet;

                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }
        }

        void IParent.AppendPath(StringBuilder builder)
        {
            this.AppendPath(builder);
        }

        internal event EventHandler<EventArgs> HasChangesSet;

        internal void Read(EmberReader reader, IDictionary<int, IInvocationResult> pendingInvocations)
        {
            reader.ReadAndAssertOuter(GlowGlobal.Root.OuterId);

            switch (reader.InnerNumber)
            {
                case GlowRootElementCollection.InnerNumber:
                    this.ReadChildren(reader);
                    break;
                case GlowInvocationResult.InnerNumber:
                    ReadInvocationResult(reader, pendingInvocations);
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        internal sealed override bool GetIsRoot()
        {
            return true;
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Method is not public, CA bug?")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "RootElement", Justification = "Official Glow name.")]
        internal sealed override bool ReadChildrenCore(EmberReader reader)
        {
            var isEmpty = true;

            while (reader.Read() && (reader.InnerNumber != InnerNumber.EndContainer))
            {
                switch (reader.InnerNumber)
                {
                    case GlowParameter.InnerNumber:
                        isEmpty = false;
                        this.ReadChild(reader, ElementType.Parameter);
                        break;
                    case GlowNode.InnerNumber:
                        isEmpty = false;
                        this.ReadChild(reader, ElementType.Node);
                        break;
                    case GlowFunction.InnerNumber:
                        isEmpty = false;
                        this.ReadChild(reader, ElementType.Function);
                        break;
                    case GlowQualifiedParameter.InnerNumber:
                        isEmpty = false;
                        this.ReadQualifiedChild(reader, ElementType.Parameter);
                        break;
                    case GlowQualifiedNode.InnerNumber:
                        isEmpty = false;
                        this.ReadQualifiedChild(reader, ElementType.Node);
                        break;
                    case GlowQualifiedFunction.InnerNumber:
                        isEmpty = false;
                        this.ReadQualifiedChild(reader, ElementType.Function);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            return isEmpty;
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Method is not public, CA bug?")]
        internal sealed override void WriteChildrenQuery(EmberWriter writer)
        {
            writer.WriteStartApplicationDefinedType(GlowGlobal.Root.OuterId, GlowRootElementCollection.InnerNumber);
            this.WriteChildrenQueryCollection(writer);
            writer.WriteEndContainer();
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Method is not public, CA bug?")]
        internal sealed override void WriteChanges(EmberWriter writer, IInvocationCollection invocationCollection)
        {
            if (this.HasChanges)
            {
                writer.WriteStartApplicationDefinedType(GlowGlobal.Root.OuterId, GlowRootElementCollection.InnerNumber);
                this.WriteChangesCollection(writer, invocationCollection);
                writer.WriteEndContainer();
                this.HasChanges = false;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Initializes a new instance of the <see cref="Root{TMostDerived}"/> class.</summary>
        /// <remarks>
        /// <para>Objects of subtypes are not created by client code directly but indirectly when a
        /// <see cref="Consumer{T}"/> object is created.</para>
        /// </remarks>
        protected Root()
        {
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static void ReadInvocationResult(EmberReader reader, IDictionary<int, IInvocationResult> pendingInvocations)
        {
            int invocationId = 0;
            bool success = true;

            while (reader.Read() && (reader.InnerNumber != InnerNumber.EndContainer))
            {
                switch (reader.GetContextSpecificOuterNumber())
                {
                    case GlowInvocationResult.InvocationId.OuterNumber:
                        invocationId = (int)reader.AssertAndReadContentsAsInt32();
                        break;
                    case GlowInvocationResult.Success.OuterNumber:
                        success = reader.ReadContentsAsBoolean();
                        break;
                    case GlowInvocationResult.Result.OuterNumber:
                        IInvocationResult result;

                        if (pendingInvocations.TryGetValue(invocationId, out result))
                        {
                            result.Read(reader, success);
                            pendingInvocations.Remove(invocationId);
                        }
                        else
                        {
                            reader.Skip();
                        }

                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }
        }

        private void ReadQualifiedChild(EmberReader reader, ElementType actualType)
        {
            reader.ReadAndAssertOuter(GlowQualifiedNode.Path.OuterId);
            var path = reader.AssertAndReadContentsAsInt32Array();

            if (path.Length == 0)
            {
                throw new ModelException("Invalid path for a qualified element.");
            }

            this.ReadQualifiedChild(reader, actualType, path, 0);
        }
    }
}
