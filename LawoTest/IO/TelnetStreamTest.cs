////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.IO
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using UnitTesting;

    /// <summary>Tests <see cref="TelnetStream"/>.</summary>
    [TestClass]
    public class TelnetStreamTest : TestBase
    {
        /// <summary>Tests the main use case.</summary>
        [TestMethod]
        [TestCategory("Manual")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Test method must be an instance method.")]
        public async Task MainTest()
        {
            var client = new TcpClient();
            await client.ConnectAsync("192.168.1.201", 23);
            var networkStream = client.GetStream();

            using (var stream = new TelnetStream(networkStream.ReadAsync, networkStream.WriteAsync, () => networkStream.DataAvailable))
            using (var reader = new StreamReader(stream, Encoding.ASCII))
            using (var writer = new StreamWriter(stream, Encoding.ASCII))
            {
                if (await WaitForPrompt(reader, "login:"))
                {
                    await writer.WriteLineAsync("root");
                    await writer.FlushAsync();

                    if (await WaitForPrompt(reader, "Password:"))
                    {
                        await writer.WriteLineAsync("hong");
                        await writer.FlushAsync();
                        await WaitForPrompt(reader, "$");
                    }
                }
            }
        }

        private static async Task<bool> WaitForPrompt(StreamReader reader, string prompt)
        {
            var buffer = new char[1024];
            int read;
            var readString = string.Empty;

            while ((read = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                readString += new string(buffer, 0, read);

                if (readString.TrimEnd().EndsWith(prompt))
                {
                    Console.Write(readString);
                    return true;
                }
            }

            Console.Write(readString);
            return false;
        }
    }
}
