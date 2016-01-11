////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2016 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System;
    using System.Collections.ObjectModel;

    using Ember;

    internal static class DynamicNodeHelper
    {
        internal static Element ReadDynamicChildContents(
            EmberReader reader,
            ElementType actualType,
            Context context,
            out RequestState childRequestState)
        {
            switch (actualType)
            {
                case ElementType.Parameter:
                    return DynamicParameter.ReadContents(reader, actualType, context, out childRequestState);
                case ElementType.Node:
                    return DynamicNode.ReadContents(reader, actualType, context, out childRequestState);
                default:
                    return DynamicFunction.ReadContents(reader, actualType, context, out childRequestState);
            }
        }

        internal static bool ChangeOnlineStatus(
            Func<IElement, bool> baseImpl, ObservableCollection<IElement> dynamicChildren, IElement child)
        {
            if (!baseImpl(child))
            {
                if (child.IsOnline)
                {
                    dynamicChildren.Add(child);
                }
                else
                {
                    dynamicChildren.Remove(child);
                }
            }

            return true;
        }
    }
}
