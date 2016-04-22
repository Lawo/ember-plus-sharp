////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2016 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    /// <summary>Represents the policy how children for a given node should be requested from the provider.</summary>
    public enum ChildrenRequestPolicy
    {
        /// <summary>Do not request any children.</summary>
        /// <remarks>No children will be requested as long as the <see cref="INode.ChildrenRequestPolicy"/>
        /// property of an <see cref="INode"/> implementation has this value.</remarks>
        None,

        /// <summary>Request only direct children.</summary>
        /// <remarks>If the <see cref="INode.ChildrenRequestPolicy"/> property of an <see cref="INode"/> implementation
        /// has this value then only direct children were or will be requested for the node. The
        /// <see cref="INode.ChildrenRequestPolicy"/> property of the direct children implementing <see cref="INode"/>
        /// will have the initial value <see cref="None"/>.</remarks>
        DirectOnly,

        /// <summary>Request direct and indirect children.</summary>
        /// <remarks>If the <see cref="INode.ChildrenRequestPolicy"/> property of an <see cref="INode"/> implementation
        /// has this value, then all direct and indirect children were or will be requested for the node. The
        /// <see cref="INode.ChildrenRequestPolicy"/> property of the direct and indirect children implementing
        /// <see cref="INode"/> will have the value <see cref="All"/>.</remarks>
        All
    }
}
