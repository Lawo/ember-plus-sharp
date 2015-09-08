////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model
{
    using System.Collections.Generic;

    /// <summary>Represents an element with schemas in the protocol specified in the
    /// <a href="http://ember-plus.googlecode.com/svn/trunk/documentation/Ember+%20Documentation.pdf">Ember+
    /// Specification</a>.</summary>
    public interface IElementWithSchemas : IElement
    {
        /// <summary>Gets <b>schemaIdentifiers</b>.</summary>
        IReadOnlyList<string> SchemaIdentifiers { get; }
    }
}
