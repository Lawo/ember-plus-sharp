////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.Threading.Tasks
{
    using System;
    using System.Threading.Tasks;

    using Lawo.UnitTesting;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Tests the <see cref="TaskQueue"/> class.</summary>
    [TestClass]
    public sealed class TaskSingletonTest : TestBase
    {
        /// <summary>Tests the main use cases.</summary>
        [TestCategory("Unattended")]
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
        [TestCategory("Unattended")]
        [TestMethod]
        public void ExceptionTest()
        {
            var queue = new TaskSingleton();

            AssertThrow<ArgumentNullException>(
                () => queue.Execute((Func<Task>)null),
                () => queue.Execute((Func<Task<string>>)null));
        }
    }
}
