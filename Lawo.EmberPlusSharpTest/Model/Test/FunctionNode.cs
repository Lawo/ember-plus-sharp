////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model.Test
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
