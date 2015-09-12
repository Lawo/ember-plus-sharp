////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model
{
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>Provides the common interface for all elements in the object tree accessible through
    /// <see cref="Consumer{T}.Root">Consumer&lt;TRoot&gt;.Root</see>.</summary>
    public interface IElement : INotifyPropertyChanged
    {
        /// <summary>Gets the parent of this element.</summary>
        /// <value>The parent of the element if it is not the root element; otherwise <c>null</c>.</value>
        /// <remarks>Is <c>null</c> for the value of <see cref="IMatrix.Parameters">IMatrix.Parameters</see> and
        /// <see cref="MatrixLabels"/> elements in the <see cref="IMatrix.Labels">IMatrix.Labels</see> collection.
        /// </remarks>
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
