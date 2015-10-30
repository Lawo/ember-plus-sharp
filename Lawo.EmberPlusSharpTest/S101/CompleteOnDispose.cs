////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.S101
{
    using System;
    using System.Threading.Tasks;

    internal sealed class CompleteOnDispose : IDisposable
    {
        private readonly TaskCompletionSource<int> source = new TaskCompletionSource<int>();

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public void Dispose()
        {
            this.source.TrySetException(new OperationCanceledException());
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal Task<int> Task
        {
            get { return this.source.Task; }
        }
    }
}
