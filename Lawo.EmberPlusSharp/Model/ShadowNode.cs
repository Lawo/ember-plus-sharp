////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2016 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System.Diagnostics.CodeAnalysis;
    using Ember;

    /// <summary>Represents a node that is invisible from the public interface.</summary>
    /// <remarks>Shadow nodes are used to process elements that are referenced through basePath fields in matrices.
    /// </remarks>
    internal sealed class ShadowNode : NodeBase<ShadowNode>
    {
        internal ShadowNode()
        {
            this.IsOnlineChangeStatus = IsOnlineChangeStatus.Unchanged;
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", Justification = "Official Glow name.")]
        internal sealed override RequestState ReadContents(EmberReader reader, ElementType actualType)
        {
            throw new ModelException("Unexpected contents for intermediate node in basePath.");
        }

        internal sealed override void WriteChanges(EmberWriter writer, IInvocationCollection pendingInvocations)
        {
            this.HasChanges = false;
        }
    }
}
