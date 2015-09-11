////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright 2008-2012 Andreas Huber Doenni, original from http://phuse.codeplex.com.
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo
{
    using System;

    using Lawo.UnitTesting;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Tests the <see cref="ScopeGuard"/> class.</summary>
    [TestClass]
    public sealed class ScopeGuardTest : TestBase
    {
        /// <summary>Tests the normal course of events.</summary>
        [TestCategory("Unattended")]
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
        [TestCategory("Unattended")]
        [TestMethod]
        public void FailureTest()
        {
            using (var disposable = new Disposable())
            {
                ScopeGuard<Disposable> disposableGuard = ScopeGuard.Create(disposable);
                disposableGuard.Dispose();
                Assert.IsTrue(disposable.DisposeCalled);
                AssertThrow<ObjectDisposedException>(delegate { Disposable test = disposableGuard.Resource; });
                disposableGuard.Dispose();
                AssertThrow<ObjectDisposedException>(delegate { Disposable test = disposableGuard.Resource; });
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
            private bool disposeCalled;

            /// <inheritdoc/>
            public void Dispose()
            {
                this.disposeCalled = true;
            }

            internal bool DisposeCalled
            {
                get { return this.disposeCalled; }
            }
        }
    }
}
