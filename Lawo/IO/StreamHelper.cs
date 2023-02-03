////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.IO
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>Provides common stream algorithms.</summary>
    /// <threadsafety static="true" instance="false"/>
    public static class StreamHelper
    {
        /// <summary>Repeatedly calls <paramref name="read"/> until <paramref name="count"/> bytes have been read into
        /// <paramref name="buffer"/>.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="read"/> equals <c>null</c>.</exception>
        /// <exception cref="EndOfStreamException">The end of the stream has been reached before
        /// <paramref name="buffer"/> could be filled to <paramref name="count"/> bytes.</exception>
        /// <exception cref="Exception"><paramref name="read"/> has thrown an exception.</exception>
        public static void Fill(ReadCallback read, byte[] buffer, int offset, int count)
        {
            if (count != TryFill(read, buffer, offset, count))
            {
                throw new EndOfStreamException("Unexpected end of stream.");
            }
        }

        /// <summary>Repeatedly calls <paramref name="read"/> until <paramref name="count"/> bytes have been read into
        /// <paramref name="buffer"/> or the end of the stream has been reached.</summary>
        /// <returns>The number of bytes read.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="read"/> equals <c>null</c>.</exception>
        /// <exception cref="Exception"><paramref name="read"/> has thrown an exception.</exception>
        public static int TryFill(ReadCallback read, byte[] buffer, int offset, int count)
        {
            if (read == null)
            {
                throw new ArgumentNullException(nameof(read));
            }

            int index = offset;
            int readCount;

            while ((readCount = read(buffer, index, count)) > 0)
            {
                index += readCount;
                count -= readCount;
            }

            return index - offset;
        }

        /// <summary>Repeatedly calls <paramref name="read"/> until <paramref name="count"/> bytes have been read into
        /// <paramref name="buffer"/>.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="read"/> equals <c>null</c>.</exception>
        /// <exception cref="EndOfStreamException">The end of the stream has been reached before
        /// <paramref name="buffer"/> could be filled to <paramref name="count"/> bytes.</exception>
        /// <exception cref="Exception"><paramref name="read"/> has thrown an exception.</exception>
        public static async Task FillAsync(
            ReadAsyncCallback read, byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (count != await TryFillAsync(read, buffer, offset, count, cancellationToken).ConfigureAwait(false))
            {
                throw new EndOfStreamException("Unexpected end of stream.");
            }
        }

        /// <summary>Repeatedly calls <paramref name="read"/> until <paramref name="count"/> bytes have been read into
        /// <paramref name="buffer"/> or the end of the stream has been reached.</summary>
        /// <returns>A task that represents the asynchronous operation. The value of the <see cref="Task{T}.Result"/>
        /// contains the number of bytes read.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="read"/> equals <c>null</c>.</exception>
        /// <exception cref="Exception"><paramref name="read"/> has thrown an exception.</exception>
        public static async Task<int> TryFillAsync(
            ReadAsyncCallback read, byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (read == null)
            {
                throw new ArgumentNullException(nameof(read));
            }

            int index = offset;
            int readCount;

            while ((readCount = await read(buffer, index, count, cancellationToken).ConfigureAwait(false)) > 0)
            {
                index += readCount;
                count -= readCount;
            }

            return index - offset;
        }
    }
}
