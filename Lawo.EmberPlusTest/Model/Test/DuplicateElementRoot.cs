////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model.Test
{
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated through reflection.")]
    internal sealed class DuplicateElementRoot : Root<DuplicateElementRoot>
    {
        internal BooleanParameter Whatever1 { get; private set; }

        [Element(Identifier = "Whatever1")]
        internal BooleanParameter Whatever2 { get; private set; }
    }
}
