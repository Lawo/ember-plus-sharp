////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.Threading.Tasks
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using UnitTesting;

    /// <summary>Tests the <see cref="TaskQueue"/> class.</summary>
    [TestClass]
    public sealed class TaskQueueTest : TestBase
    {
        /// <summary>Tests the main use cases.</summary>
        [TestMethod]
        public void MainTest()
        {
            AsyncPump.Run(
                async () =>
                {
                    var counter = 0;
                    var queue = new TaskQueue();
                    queue.Enqueue(
                        async () =>
                        {
                            await Task.Delay(250);
                            ++counter;
                        }).Ignore();
                    Assert.AreEqual(2, await queue.Enqueue(() => Task.FromResult(++counter)));
                    Assert.AreEqual(2, counter);
                });
        }

        /// <summary>Tests the exceptional cases.</summary>
        [TestMethod]
        public void ExceptionTest()
        {
            var queue = new TaskQueue();

            AssertThrow<ArgumentNullException>(
                () => queue.Enqueue(null),
                () => queue.Enqueue((Func<Task<string>>)null));
        }
    }
}
