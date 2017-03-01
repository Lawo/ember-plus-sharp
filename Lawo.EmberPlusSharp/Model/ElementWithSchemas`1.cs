////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System;
    using System.Collections.Generic;

    using Ember;
    using Glow;

    /// <summary>This class is not intended to be referenced in your code.</summary>
    /// <remarks>Provides the common implementation for all elements with schemas in the object tree accessible through
    /// <see cref="Consumer{T}.Root">Consumer&lt;TRoot&gt;.Root</see>.</remarks>
    /// <typeparam name="TMostDerived">The most-derived subtype of this class.</typeparam>
    /// <threadsafety static="true" instance="false"/>
    public abstract class ElementWithSchemas<TMostDerived> : Element<TMostDerived>, IElementWithSchemas
        where TMostDerived : ElementWithSchemas<TMostDerived>
    {
        /// <summary>Gets <b>schemaIdentifiers</b>.</summary>
        public IReadOnlyList<string> SchemaIdentifiers
        {
            get { return this.schemaIdentifiers; }
            private set { this.SetValue(ref this.schemaIdentifiers, value); }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal ElementWithSchemas(RetrievalState detailsRetrievalState)
        {
            this.detailsRetrievalState = detailsRetrievalState;
        }

        /// <summary>Gets or sets the retrieval state.</summary>
        /// <remarks>This implementation has nothing to do with schemas. However, it so happens that this member has the
        /// same behavior for all subclasses (parameters, nodes and matrices). If this fact ever changes, it probably
        /// makes sense to move this member to its own base class (named e.g. RequestedElement).</remarks>
        internal sealed override RetrievalState RetrievalState
        {
            get
            {
                return this.RetrieveDetails ? this.detailsRetrievalState : this.noDetailsRetrievalState;
            }

            set
            {
                if (this.RetrieveDetails)
                {
                    this.detailsRetrievalState = value;
                }
                else
                {
                    this.noDetailsRetrievalState = value;
                }
            }
        }

        internal override RetrievalState ReadAdditionalField(EmberReader reader, int contextSpecificOuterNumber)
        {
            reader.Skip();
            return this.RetrievalState;
        }

        internal override RetrievalState UpdateRetrievalState(bool throwForMissingRequiredChildren)
        {
            if (!this.RetrieveDetails || (this.RetrievalState.Equals(RetrievalState.Complete) &&
                this.AreRequiredChildrenAvailable(throwForMissingRequiredChildren)))
            {
                this.RetrievalState = base.UpdateRetrievalState(throwForMissingRequiredChildren);
            }

            return this.RetrievalState;
        }

        internal override void SetComplete()
        {
            this.RetrievalState = RetrievalState.Complete;
            base.SetComplete();
        }

        internal void WriteCommandCollection(
            EmberWriter writer, GlowCommandNumber commandNumber, RetrievalState retrievalState)
        {
            writer.WriteStartApplicationDefinedType(GlowElementCollection.Element.OuterId, GlowCommand.InnerNumber);
            writer.WriteValue(GlowCommand.Number.OuterId, (long)commandNumber);
            writer.WriteEndContainer();
            this.RetrievalState = retrievalState;
        }

        internal void ReadSchemaIdentifiers(EmberReader reader) =>
            this.SchemaIdentifiers = reader.AssertAndReadContentsAsString().Split('\n');

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>See <see cref="RetrievalState"/> for more information.</summary>
        /// <remarks>This field and its sibling <see cref="noDetailsRetrievalState"/> are modified by the following
        /// methods, which are directly or indirectly called from
        /// <see cref="Consumer{T}.CreateAsync(Lawo.EmberPlusSharp.S101.S101Client)"/>:
        /// <list type="number">
        /// <item><see cref="Element.UpdateRetrievalState"/></item>
        /// <item><see cref="Element.WriteRequest"/></item>
        /// <item><see cref="Element.ReadChildren"/></item>
        /// <item><see cref="Element.AreRequiredChildrenAvailable"/></item>
        /// </list>
        /// See individual method documentation for semantics. This rather complex system was implemented to make the
        /// process of retrieving elements from the provider as efficient as possible, namely:
        /// <list type="bullet">
        /// <item>As few as possible messages are sent to request children and/or subscribe to streams.</item>
        /// <item>The computational effort for tree traversal is kept as low as possible. This is necessary because all
        /// code is always executed on the applications GUI thread. Without these optimizations, a full tree traversal
        /// would be necessary after each processed message. Some providers send a new message for each updated
        /// parameter, which would very quickly lead to significant CPU load and an unresponsive GUI if many parameters
        /// are changed at once in a large tree.</item>
        /// </list>
        /// </remarks>
        private RetrievalState noDetailsRetrievalState = RetrievalState.Complete;
        private RetrievalState detailsRetrievalState;

        private IReadOnlyList<string> schemaIdentifiers;
    }
}
