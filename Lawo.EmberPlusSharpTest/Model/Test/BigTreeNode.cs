////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model.Test
{
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated through reflection.")]
    internal sealed class BigTreeNode : FieldNode<BigTreeNode>
    {
        [Element(IsOptional = true)]
        internal BooleanParameter BooleanParameter { get; private set; }

        [Element(IsOptional = true)]
        internal IntegerParameter IntegerParameter { get; private set; }

        [Element(IsOptional = true)]
        internal IntegerParameter FactorIntegerParameter { get; private set; }

        [Element(IsOptional = true)]
        internal IntegerParameter FormulaIntegerParameter { get; private set; }

        [Element(IsOptional = true)]
        internal EnumParameter<Enumeration> EnumMapParameter { get; private set; }

        [Element(IsOptional = true)]
        internal OctetstringParameter OctetstringParameter { get; private set; }

        [Element(IsOptional = true)]
        internal RealParameter RealParameter { get; private set; }

        [Element(IsOptional = true)]
        internal StringParameter StringParameter { get; private set; }

        [Element(IsOptional = true)]
        internal BigTreeNode Node1 { get; private set; }

        [Element(IsOptional = true)]
        internal BigTreeNode Node2 { get; private set; }

        [Element(IsOptional = true)]
        internal BigTreeNode Node3 { get; private set; }

        [Element(IsOptional = true)]
        internal BigTreeNode Node4 { get; private set; }

        [Element(IsOptional = true)]
        internal BigTreeNode Node5 { get; private set; }
    }
}
