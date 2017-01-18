////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System.Collections.Generic;

    internal sealed class InvocationCollection : Dictionary<int, IInvocationResult>, IInvocationCollection
    {
        int IInvocationCollection.Add(IInvocationResult invocationResult)
        {
            this.Add(++this.lastInvocationId, invocationResult);
            return this.lastInvocationId;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private int lastInvocationId;
    }
}
