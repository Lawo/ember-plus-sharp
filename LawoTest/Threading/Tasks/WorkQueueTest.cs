////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.Threading.Tasks
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using UnitTesting;

    /// <summary>Tests the <see cref="WorkQueue"/> class.</summary>
    [TestClass]
    public sealed class WorkQueueTest : TestBase
    {
        /// <summary>Tests the main use cases.</summary>
        [TestMethod]
        public void MainTest()
        {
            var cancelToken = new CancellationTokenSource().Token;
            AsyncPump.Run(
                async () =>
                {
                    var counter = 0;
                    var queue = new WorkQueue();
                    queue.Enqueue(() => ++counter).Ignore();
                    await queue.Enqueue(() => ++counter);
                    Assert.AreEqual(2, counter);
                }, cancelToken);
        }

        /// <summary>Tests the exceptional cases.</summary>
        [TestMethod]
        public void ExceptionTest()
        {
            var queue = new WorkQueue();

            AssertThrow<ArgumentNullException>(
                () => queue.Enqueue(null),
                () => queue.Enqueue((Func<string>)null));
        }
    }
}
