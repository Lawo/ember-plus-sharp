////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>Represents a matrix label entry.</summary>
    /// <threadsafety static="true" instance="false"/>
    [SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Default equality implementation is fine.")]
    public struct MatrixLabel
    {
        /// <summary>Gets <c>basePath</c>.</summary>
        public IReadOnlyList<int> BasePath { get; }

        /// <summary>Gets <c>description</c>.</summary>
        public string Description { get; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal MatrixLabel(IReadOnlyList<int> basePath, string description)
        {
            this.BasePath = basePath;
            this.Description = description;
        }
    }
}
