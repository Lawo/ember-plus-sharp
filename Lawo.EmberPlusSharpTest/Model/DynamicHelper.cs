////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System.Linq;

    internal static class DynamicHelper
    {
        internal static IElement GetChild<TMostDerived>(this DynamicFieldNode<TMostDerived> node, string identifier)
            where TMostDerived : DynamicFieldNode<TMostDerived>
        {
            return node.DynamicChildren.FirstOrDefault(c => c.Identifier == identifier);
        }

        internal static IElement GetChild<TMostDerived>(this DynamicRoot<TMostDerived> root, string identifier)
            where TMostDerived : DynamicRoot<TMostDerived>
        {
            return root.DynamicChildren.FirstOrDefault(c => c.Identifier == identifier);
        }
    }
}
