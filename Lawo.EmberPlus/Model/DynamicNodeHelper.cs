////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model
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
            out ChildrenState childChildrenState)
        {
            switch (actualType)
            {
                case ElementType.Parameter:
                    return DynamicParameter.ReadContents(reader, actualType, context, out childChildrenState);
                case ElementType.Node:
                    return DynamicNode.ReadContents(reader, actualType, context, out childChildrenState);
                default:
                    return DynamicFunction.ReadContents(reader, actualType, context, out childChildrenState);
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
