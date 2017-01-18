////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model.Test
{
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated through reflection.")]
    internal sealed class InterfaceElementRoot<TCollectionNode> : Root<InterfaceElementRoot<TCollectionNode>>
        where TCollectionNode : INode
    {
        internal IParameter BooleanParameter { get; private set; }

        internal IParameter IntegerParameter { get; private set; }

        internal IParameter FactorIntegerParameter { get; private set; }

        internal IParameter FormulaIntegerParameter { get; private set; }

        internal IParameter EnumerationParameter { get; private set; }

        internal IParameter EnumMapParameter { get; private set; }

        internal IParameter OctetstringParameter { get; private set; }

        internal IParameter RealParameter { get; private set; }

        internal IParameter StringParameter { get; private set; }

        internal INode FieldNode { get; private set; }

        internal TCollectionNode CollectionNode { get; private set; }
    }
}
