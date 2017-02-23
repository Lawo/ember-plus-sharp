////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
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
            out RetrievalState childRetrievalState)
        {
            switch (actualType)
            {
                case ElementType.Parameter:
                    return DynamicParameter.ReadContents(reader, actualType, context, out childRetrievalState);
                case ElementType.Node:
                    return DynamicNode.ReadContents(reader, actualType, context, out childRetrievalState);
                case ElementType.Function:
                    return DynamicFunction.ReadContents(reader, actualType, context, out childRetrievalState);
                default:
                    return DynamicMatrix.ReadContents(reader, actualType, context, out childRetrievalState);
            }
        }

        internal static bool ChangeVisibility(
            Func<IElement, bool> baseImpl, ObservableCollection<IElement> dynamicChildren, IElement child)
        {
            if (!baseImpl(child))
            {
                VisibilityHelper.ChangeVisibility(dynamicChildren, child);
            }

            return true;
        }
    }
}
