////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    /// <summary>Represents a matrix label entry.</summary>
    /// <threadsafety static="true" instance="false"/>
    public struct MatrixLabel
    {
        /// <summary>Gets <c>basePath</c>.</summary>
        public int[] BasePath { get; }

        /// <summary>Gets <c>description</c>.</summary>
        public string Description { get; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal MatrixLabel(int[] basePath, string description)
        {
            this.BasePath = basePath;
            this.Description = description;
        }
    }
}
