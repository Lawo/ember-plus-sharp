////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System.Collections.Generic;

    internal static class VisibilityHelper
    {
        internal static void ChangeVisibility<T>(ICollection<T> children, T child)
            where T : IElement
        {
            if (child.IsOnline)
            {
                if (!children.Contains(child))
                {
                    children.Add(child);
                }
            }
            else
            {
                children.Remove(child);
            }
        }
    }
}
