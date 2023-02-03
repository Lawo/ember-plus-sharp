////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.Threading.Tasks
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using UnitTesting;

    /// <summary>Tests the <see cref="AsyncPump"/> class.</summary>
    [TestClass]
    public sealed class AsyncPumpTest : TestBase
    {
        /// <summary>Tests <see cref="AsyncPump"/> main use cases.</summary>
        [TestMethod]
        public void MainTest()
        {
            LogMethodPosition("Begin");
            var cancelToken = new CancellationTokenSource().Token;
            AsyncPump.Run(DoEverything, cancelToken);
            LogMethodPosition("End");
        }

        /// <summary>Tests <see cref="AsyncPump"/> exceptions.</summary>
        [TestMethod]
        public void ExceptionTest()
        {
            var cancelToken = new CancellationTokenSource().Token;
            AssertThrow<ArgumentNullException>(() => AsyncPump.Run(null, cancelToken));
            AssertThrow<ArgumentException>(() => AsyncPump.Run(() => null, cancelToken));

            AsyncPump.Run(
                () =>
                {
                    AssertThrow<NotSupportedException>(() => SynchronizationContext.Current.Send(o => { }, null));
                    AssertThrow<ArgumentNullException>(() => SynchronizationContext.Current.Post(null, new object()));
                    return Task.FromResult(false);
                },
                cancelToken);

            AssertThrow<InvalidOperationException>(
                () => AsyncPump.Run(() => { throw new InvalidOperationException(); }, cancelToken));
        }

        private static async Task DoEverything()
        {
            LogMethodPosition("Begin");
            DoSomething();
            await DoSomethingElse();
            LogMethodPosition("End");
        }

        private static async void DoSomething()
        {
            LogMethodPosition("Begin");
            await Task.Delay(1000);
            LogMethodPosition("End");
        }

        private static async Task DoSomethingElse()
        {
            LogMethodPosition("Begin");
            await Task.Delay(500);
            LogMethodPosition("End");
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", Justification = "String is a format specifier.")]
        private static void LogMethodPosition(string position, [CallerMemberName] string methodName = null) =>
            Console.WriteLine("{0:HH:mm:ss.ff} {1}() {2}", DateTime.Now, methodName, position);
    }
}
