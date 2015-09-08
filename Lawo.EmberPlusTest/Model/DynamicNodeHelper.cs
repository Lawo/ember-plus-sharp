////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model
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
