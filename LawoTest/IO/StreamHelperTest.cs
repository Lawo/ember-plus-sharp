////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.IO
{
    using System;
    using System.Threading;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Threading.Tasks;
    using UnitTesting;

    /// <summary>Tests <see cref="StreamHelper"/>.</summary>
    [TestClass]
    public class StreamHelperTest : TestBase
    {
        /// <summary>Tests exceptional use cases.</summary>
        [TestMethod]
        public void ExceptionTest()
        {
            var cancelToken = new CancellationTokenSource().Token;
            AsyncPump.Run(
                async () =>
                {
                    AssertThrow<ArgumentNullException>(() => StreamHelper.Fill(null, new byte[1], 0, 1));
                    await AssertThrowAsync<ArgumentNullException>(
                        () => StreamHelper.FillAsync(null, new byte[1], 0, 1, CancellationToken.None));
                }, cancelToken);
        }
    }
}
