////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model.Test
{
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated through reflection.")]
    internal sealed class MainRoot : Root<MainRoot>
    {
        internal BooleanParameter BooleanParameter { get; private set; }

        internal IntegerParameter IntegerParameter { get; private set; }

        internal IntegerParameter FactorIntegerParameter { get; private set; }

        internal IntegerParameter FormulaIntegerParameter { get; private set; }

        internal EnumParameter<Enumeration> EnumerationParameter { get; private set; }

        internal EnumParameter<Enumeration> EnumMapParameter { get; private set; }

        internal OctetstringParameter OctetstringParameter { get; private set; }

        internal RealParameter RealParameter { get; private set; }

        internal StringParameter StringParameter { get; private set; }

        internal TestFieldNode FieldNode { get; private set; }

        internal CollectionNode<BooleanParameter> CollectionNode { get; private set; }
    }
}
