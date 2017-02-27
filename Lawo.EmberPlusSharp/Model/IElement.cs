////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>Provides the common interface for all elements in the object tree accessible through
    /// <see cref="Consumer{T}.Root">Consumer&lt;TRoot&gt;.Root</see>.</summary>
    public interface IElement : INotifyPropertyChanged
    {
        /// <summary>Gets the parent of this element.</summary>
        /// <value>The parent of the element if it is not the root element; otherwise <c>null</c>.</value>
        INode Parent { get; }

        /// <summary>Gets <b>number</b>.</summary>
        int Number { get; }

        /// <summary>Gets <b>identifier</b>.</summary>
        string Identifier { get; }

        /// <summary>Gets <b>description</b>.</summary>
        string Description { get; }

        /// <summary>Gets a value indicating whether this element is online.</summary>
        bool IsOnline { get; }

        /// <summary>Gets or sets an arbitrary object value that can be used to store custom information about this
        /// object.</summary>
        object Tag { get; set; }

        /// <summary>Gets the full path of this element.</summary>
        /// <remarks>The path is assembled by joining the identifiers of this element and all direct and indirect
        /// parents.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Expensive method.")]
        string GetPath();
    }
}
