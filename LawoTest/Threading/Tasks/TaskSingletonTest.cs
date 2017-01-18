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
    public sealed class TaskSingletonTest : TestBase
    {
        /// <summary>Tests the main use cases.</summary>
        [TestMethod]
        public void MainTest()
        {
            AsyncPump.Run(
                async () =>
                {
                    var singleton = new TaskSingleton();
                    var task1 = singleton.Execute(() => Task.Delay(250));
                    await AssertThrowAsync<InvalidOperationException>(() => singleton.Execute(() => Task.Delay(250)));
                    await task1;
                    var value = this.Random.Next();
                    Assert.AreEqual(value, await singleton.Execute(() => Task.FromResult(value)));
                });
        }

        /// <summary>Tests the exceptional cases.</summary>
        [TestMethod]
        public void ExceptionTest()
        {
            var queue = new TaskSingleton();

            AssertThrow<ArgumentNullException>(
                () => queue.Execute(null),
                () => queue.Execute((Func<Task<string>>)null));
        }
    }
}
