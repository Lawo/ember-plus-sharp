////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model.Test
{
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated through reflection.")]
    internal sealed class StreamRoot : Root<StreamRoot>
    {
        internal BooleanParameter BooleanParameter { get; private set; }

        internal IntegerParameter IntegerParameter { get; private set; }

        internal EnumParameter<Enumeration> EnumerationParameter { get; private set; }

        internal OctetstringParameter OctetstringParameter { get; private set; }

        internal RealParameter RealParameter { get; private set; }

        internal StringParameter StringParameter { get; private set; }
    }
}
