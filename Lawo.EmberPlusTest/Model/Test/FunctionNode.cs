////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model.Test
{
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated through reflection.")]
    internal sealed class FunctionNode : FieldNode<FunctionNode>
    {
        internal Function<long, long, long, long, Result<long, long, long, long>> Function4 { get; private set; }

        internal Function<long, long, long, long, long, Result<long, long, long, long, long>> Function5 { get; private set; }

        internal Function<long, long, long, long, long, long, Result<long, long, long, long, long, long>> Function6 { get; private set; }
    }
}
