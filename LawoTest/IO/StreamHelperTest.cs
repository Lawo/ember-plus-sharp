////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.IO
{
    using System;
    using System.Threading;

    using Lawo.Threading.Tasks;
    using Lawo.UnitTesting;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Tests <see cref="StreamHelper"/>.</summary>
    [TestClass]
    public class StreamHelperTest : TestBase
    {
        /// <summary>Tests exceptional use cases.</summary>
        [TestMethod]
        public void ExceptionTest()
        {
            AsyncPump.Run(
                async () =>
                {
                    AssertThrow<ArgumentNullException>(() => StreamHelper.Fill((ReadCallback)null, new byte[1], 0, 1));
                    await AssertThrowAsync<ArgumentNullException>(
                        () => StreamHelper.FillAsync((ReadAsyncCallback)null, new byte[1], 0, 1, CancellationToken.None));
                });
        }
    }
}
