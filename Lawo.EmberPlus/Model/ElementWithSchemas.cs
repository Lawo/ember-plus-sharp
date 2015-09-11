////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model
{
    using System.Collections.Generic;
    using Lawo.EmberPlus.Ember;

    /// <summary>Represents an element with schemas in the protocol specified in the
    /// <a href="http://ember-plus.googlecode.com/svn/trunk/documentation/Ember+%20Documentation.pdf">Ember+
    /// Specification</a>.</summary>
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
