////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model
{
    using System.Collections.Generic;

    using Ember;

    /// <summary>This class is not intended to be referenced in your code.</summary>
    /// <remarks>Provides the common implementation for all elements with schemas in the object tree accessible through
    /// <see cref="Consumer{T}.Root">Consumer&lt;TRoot&gt;.Root</see>.</remarks>
    /// <typeparam name="TMostDerived">The most-derived subtype of this class.</typeparam>
    /// <threadsafety static="true" instance="false"/>
    public abstract class ElementWithSchemas<TMostDerived> : Element<TMostDerived>, IElementWithSchemas
        where TMostDerived : ElementWithSchemas<TMostDerived>
    {
        private IReadOnlyList<string> schemaIdentifiers;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets <b>schemaIdentifiers</b>.</summary>
        public IReadOnlyList<string> SchemaIdentifiers
        {
            get { return this.schemaIdentifiers; }
            private set { this.SetValue(ref this.schemaIdentifiers, value); }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal ElementWithSchemas()
        {
        }

        internal void ReadSchemaIdentifiers(EmberReader reader)
        {
            this.SchemaIdentifiers = reader.AssertAndReadContentsAsString().Split('\n');
        }
    }
}
