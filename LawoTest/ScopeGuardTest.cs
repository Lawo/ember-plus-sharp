////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright 2008-2012 Andreas Huber Doenni, original from http://phuse.codeplex.com.
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using UnitTesting;

    /// <summary>Tests the <see cref="ScopeGuard"/> class.</summary>
    [TestClass]
    public sealed class ScopeGuardTest : TestBase
    {
        /// <summary>Tests the normal course of events.</summary>
        [TestMethod]
        public void NormalTest()
        {
            using (var disposable = new Disposable())
            using (var disposableGuard = ScopeGuard.Create(disposable))
            {
                AssertNotDisposed(disposable, disposableGuard);
                disposableGuard.Dismiss();
                AssertNotDisposed(disposable, disposableGuard);
                disposableGuard.Dispose();
                AssertNotDisposed(disposable, disposableGuard);
            }
        }

        /// <summary>Tests what happens in the event of a failure.</summary>
        [TestMethod]
        public void FailureTest()
        {
            using (var disposable = new Disposable())
            {
                ScopeGuard<Disposable> disposableGuard = ScopeGuard.Create(disposable);
                disposableGuard.Dispose();
                Assert.IsTrue(disposable.DisposeCalled);
                AssertThrow<ObjectDisposedException>(delegate { disposableGuard.Resource.Ignore(); });
                disposableGuard.Dispose();
                AssertThrow<ObjectDisposedException>(delegate { disposableGuard.Resource.Ignore(); });
                AssertThrow<ObjectDisposedException>(delegate { disposableGuard.Dismiss(); });
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static void AssertNotDisposed(Disposable disposable, ScopeGuard<Disposable> disposableGuard)
        {
            Assert.IsFalse(disposable.DisposeCalled);
            Assert.AreSame(disposable, disposableGuard.Resource);
            Assert.IsFalse(disposable.DisposeCalled);
        }

        /// <summary>Mock used for testing <see cref="ScopeGuard"/>.</summary>
        private sealed class Disposable : IDisposable
        {
            /// <inheritdoc/>
            public void Dispose() => this.DisposeCalled = true;

            internal bool DisposeCalled { get; private set; }
        }
    }
}
