////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model
{
    using System.Collections.Generic;

    /// <summary>Represents an element with schemas in the object tree accessible through
    /// <see cref="Consumer{T}.Root">Consumer&lt;TRoot&gt;.Root</see>.</summary>
    public interface IElementWithSchemas : IElement
    {
        /// <summary>Gets <b>schemaIdentifiers</b>.</summary>
        IReadOnlyList<string> SchemaIdentifiers { get; }
    }
}
