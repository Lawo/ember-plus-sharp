////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright 2008-2012 Andreas Huber Doenni, original from http://phuse.codeplex.com.
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.IO
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>References a method to be called when bytes need to be read from a source.</summary>
    /// <remarks>The referenced method must behave like <see cref="Stream.Read(byte[], int, int)"/>. Notably,
    /// calling code will assume that <see cref="ObjectDisposedException"/> is thrown when the object representing the
    /// source has been disposed.</remarks>
    public delegate int ReadCallback(byte[] buffer, int offset, int count);

    /// <summary>References a method to be called when bytes need to be read from a source asynchronously.</summary>
    /// <remarks>The referenced method must behave like
    /// <see cref="Stream.ReadAsync(byte[], int, int, CancellationToken)"/>. Notably, calling code that needs to cancel
    /// a read operation will do so as follows:
    /// <list type="number">
    /// <item>It will call <see cref="CancellationTokenSource.Cancel()"/> on the object from which
    /// <paramref name="cancellationToken"/> originated and wait for an <see cref="OperationCanceledException"/> to be
    /// thrown.</item>
    /// <item>If no <see cref="OperationCanceledException"/> is thrown within say 500 milliseconds,
    /// <see cref="IDisposable.Dispose"/> is called on the object representing the source and the caller then waits
    /// for either an <see cref="OperationCanceledException"/> or an <see cref="ObjectDisposedException"/>.</item>
    /// </list>
    /// The steps above ensure that cancellation will work correctly for APIs that do support
    /// <see cref="CancellationToken"/> as well as for those that don't (where the recommended practice is to simply
    /// call <see cref="IDisposable.Dispose"/> on the object representing the source.</remarks>
    public delegate Task<int> ReadAsyncCallback(
        byte[] buffer, int offset, int count, CancellationToken cancellationToken);

    /// <summary>Provides a thin wrapper for a buffer that is filled by calling the provided callback.</summary>
    /// <remarks>
    /// <para>The fields in this class and its base are not encapsulated properly for performance reasons. For example,
    /// <see cref="Buffer.this"/> along with <see cref="Index"/> should be replaced with a method like
    /// <c>GetBufferedByte()</c>. However, doing so costs more than 30% of the throughput in dependent classes when run
    /// on the windows phone emulator. This most likely stems form the fact that the CF JIT is only able to inline very
    /// simple methods. Apparently, even a method with the one-liner <c>return this.buffer[this.index++];</c> is not
    /// eligible for inlining.</para>
    /// <para>A frequent use case for this class is when a not previously known number of bytes need to be read from a
    /// stream one by one. In this case the following code tends to be much faster than calling
    /// <see cref="Stream.ReadByte"/> for each byte:
    /// <code>
    /// void ReadFromStream(Stream stream)
    /// {
    ///     var readBuffer = new ReadBuffer(stream.Read, 1024);
    ///
    ///     while ((readBuffer.Index &lt; readBuffer.Count) || readBuffer.Read())
    ///     {
    ///         var theByte = readBuffer[readBuffer.Index++];
    ///
    ///         // Do something with the byte.
    ///     }
    /// }
    /// </code>
    /// </para>
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    public sealed class ReadBuffer : Buffer
    {
        /// <summary>Initializes a new instance of the <see cref="ReadBuffer"/> class.</summary>
        /// <param name="read">The method that is called when the buffer needs to be filled.</param>
        /// <param name="bufferSize">The size of the buffer in bytes.</param>
        /// <exception cref="ArgumentNullException"><paramref name="read"/> equals <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="bufferSize"/> is 0 or negative.</exception>
        public ReadBuffer(ReadCallback read, int bufferSize)
            : base(bufferSize)
        {
            if (read == null)
            {
                throw new ArgumentNullException(nameof(read));
            }

            this.read = read;
            this.readAsync = (b, o, c, t) => ThrowInvalidAsyncOperationException();
        }

        /// <summary>Initializes a new instance of the <see cref="ReadBuffer"/> class.</summary>
        /// <param name="readAsync">The asynchronous method that is called when the buffer needs to be filled.</param>
        /// <param name="bufferSize">The size of the buffer in bytes.</param>
        /// <exception cref="ArgumentNullException"><paramref name="readAsync"/> equals <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="bufferSize"/> is 0 or negative.</exception>
        public ReadBuffer(ReadAsyncCallback readAsync, int bufferSize)
            : base(bufferSize)
        {
            if (readAsync == null)
            {
                throw new ArgumentNullException(nameof(readAsync));
            }

            this.readAsync = readAsync;
            this.read = (b, o, c) => ThrowInvalidSyncOperationException();
        }

        /// <summary>Gets the number of bytes actually contained in the buffer.</summary>
        public int Count { get; private set; }

        /// <summary>Gets or sets the current index into the buffer.</summary>
        /// <remarks>Caution: For performance reasons, the value set for this property is not validated. It is the
        /// clients responsibility to set only valid values (value &gt;= 0 &amp;&amp; value &lt; <see cref="Count"/>).
        /// </remarks>
        public int Index { get; set; }

        /// <summary>Gets the current position within the stream.</summary>
        public long Position => this.previousPosition + this.Index;

        /// <summary>Reads bytes into the buffer by calling the callback specified during construction exactly once.
        /// </summary>
        /// <returns><c>true</c> when at least one byte was read; otherwise <c>false</c>.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="ReadBuffer"/> object was created by calling
        /// <see cref="ReadBuffer(ReadAsyncCallback, int)"/>.</exception>
        /// <remarks>
        /// <para>After each call to this method, <see cref="Count"/> contains the number of bytes in the buffer
        /// and <see cref="Index"/> equals 0.</para>
        /// <para>This function is usually called when <see cref="Index"/> equals <see cref="Count"/>, that is,
        /// when the client has processed all bytes in the buffer (the whole buffer is filled in this case). If it is
        /// called earlier (when <see cref="Index"/> &lt; <see cref="Count"/>) then the remaining bytes at the
        /// end of the buffer are first copied to the start of the buffer and the now empty part of the buffer is filled
        /// by calling the callback.</para>
        /// </remarks>
        public bool Read()
        {
            this.Compact();
            var remainingCount = this.Count;
            this.Count += this.read(this.GetBuffer(), remainingCount, this.Capacity - remainingCount);
            return this.Count > remainingCount;
        }

        /// <summary>Asynchronously reads bytes into the buffer by calling the callback specified during construction
        /// exactly once.</summary>
        /// <returns>A task that represents the asynchronous operation. The value of the <see cref="Task{T}.Result"/>
        /// property equals <c>true</c> when at least one byte was read; otherwise <c>false</c>.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="ReadBuffer"/> object was created by calling
        /// <see cref="ReadBuffer(ReadCallback, int)"/>.</exception>
        /// <remarks>
        /// <para>After the returned task completes, <see cref="Count"/> contains the number of bytes in the buffer and
        /// <see cref="Index"/> equals 0.</para>
        /// <para>This function is usually called when <see cref="Index"/> equals <see cref="Count"/>, that is,
        /// when the client has processed all bytes in the buffer (the whole buffer is filled in this case). If it is
        /// called earlier (when <see cref="Index"/> &lt; <see cref="Count"/>) then the remaining bytes at the
        /// end of the buffer are first copied to the start of the buffer and the now empty part of the buffer is filled
        /// by calling the callback.</para>
        /// </remarks>
        public async Task<bool> ReadAsync(CancellationToken cancellationToken)
        {
            this.Compact();
            var remainingCount = this.Count;
            var count = this.Capacity - remainingCount;
            this.Count += await readAsync(GetBuffer(), remainingCount, count, cancellationToken).ConfigureAwait(false);
            return this.Count > remainingCount;
        }

        /// <summary>Reads bytes from the buffer by calling the callback specified during construction at most once.
        /// </summary>
        /// <exception cref="ArgumentException">The length of <paramref name="buffer"/> is less than
        /// <paramref name="offset"/> plus <paramref name="count"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> equals <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> and/or <paramref name="count"/>
        /// are negative.</exception>
        /// <exception cref="InvalidOperationException">The <see cref="ReadBuffer"/> object was created by calling
        /// <see cref="ReadBuffer(ReadAsyncCallback, int)"/>.</exception>
        /// <remarks>The callback specified during construction is only called if <see cref="Index"/> equals
        /// <see cref="Count"/>.</remarks>
        public int Read(byte[] buffer, int offset, int count)
        {
            if (this.Index < this.Count)
            {
                return this.ReadFromBuffer(buffer, offset, count);
            }
            else
            {
                if (count > this.Capacity)
                {
                    this.Compact();
                    var readCount = this.read(buffer, offset, count);
                    this.previousPosition += readCount;
                    return readCount;
                }
                else
                {
                    this.Read();
                    return this.ReadFromBuffer(buffer, offset, count);
                }
            }
        }

        /// <summary>Asynchronously reads bytes from the buffer by calling the callback specified during construction at
        /// most once.</summary>
        /// <returns>A task that represents the asynchronous operation. The value of the <see cref="Task{T}.Result"/>
        /// property contains the total number of bytes read into the buffer. The result value can be less than the
        /// number of bytes requested if the number of bytes currently available is less than the requested number, or
        /// it can be 0 (zero) if the end of the stream has been reached.</returns>
        /// <exception cref="ArgumentException">The length of <paramref name="buffer"/> is less than
        /// <paramref name="offset"/> plus <paramref name="count"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> equals <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> and/or <paramref name="count"/>
        /// are negative.</exception>
        /// <exception cref="InvalidOperationException">The <see cref="ReadBuffer"/> object was created by calling
        /// <see cref="ReadBuffer(ReadCallback, int)"/>.</exception>
        /// <remarks>The callback specified during construction is only called if <see cref="Index"/> equals
        /// <see cref="Count"/>.</remarks>
        public async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (this.Index < this.Count)
            {
                return this.ReadFromBuffer(buffer, offset, count);
            }
            else
            {
                if (count > this.Capacity)
                {
                    this.Compact();
                    var readCount = await readAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
                    this.previousPosition += readCount;
                    return readCount;
                }
                else
                {
                    await ReadAsync(cancellationToken).ConfigureAwait(false);
                    return this.ReadFromBuffer(buffer, offset, count);
                }
            }
        }

        /// <summary>Ensures that <paramref name="count"/> &lt;= (<see cref="Count"/> - <see cref="Index"/>).
        /// </summary>
        /// <param name="count">The minimum number of bytes that will be available when this method returns.</param>
        /// <exception cref="EndOfStreamException">The end of the stream has been reached before the buffer could be
        /// filled to <paramref name="count"/> bytes.</exception>
        /// <exception cref="InvalidOperationException">The <see cref="ReadBuffer"/> object was created by calling
        /// <see cref="ReadBuffer(ReadAsyncCallback, int)"/>.</exception>
        /// <remarks>Reads bytes into the buffer by repeatedly calling the callback specified during construction until
        /// at least <paramref name="count"/> bytes are available.</remarks>
        public void Fill(int count)
        {
            if (count > this.Count - this.Index)
            {
                this.FillCore(count);
            }
        }

        /// <summary>Asynchronously ensures that <paramref name="count"/> &lt;= (<see cref="Count"/> -
        /// <see cref="Index"/>).</summary>
        /// <param name="count">The minimum number of bytes that will be available when the returned <see cref="Task"/>
        /// completes.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <exception cref="EndOfStreamException">The end of the stream has been reached before the buffer could be
        /// filled to <paramref name="count"/> bytes.</exception>
        /// <exception cref="InvalidOperationException">The <see cref="ReadBuffer"/> object was created by calling
        /// <see cref="ReadBuffer(ReadCallback, int)"/>.</exception>
        /// <remarks>Asynchronously reads bytes into the buffer by repeatedly calling the callback specified during
        /// construction until at least <paramref name="count"/> bytes are available.</remarks>
        public async Task FillAsync(int count, CancellationToken cancellationToken)
        {
            if (count > this.Count - this.Index)
            {
                await FillCoreAsync(count, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>Reads exactly <paramref name="count"/> bytes from the buffer.</summary>
        /// <exception cref="ArgumentException">The length of <paramref name="buffer"/> is less than
        /// <paramref name="offset"/> plus <paramref name="count"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> equals <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> and/or <paramref name="count"/>
        /// are negative.</exception>
        /// <exception cref="EndOfStreamException">The end of the stream has been reached before
        /// <paramref name="buffer"/> could be filled to <paramref name="count"/> bytes.</exception>
        /// <exception cref="InvalidOperationException">The <see cref="ReadBuffer"/> object was created by calling
        /// <see cref="ReadBuffer(ReadAsyncCallback, int)"/>.</exception>
        /// <remarks>If <paramref name="count"/> &gt; <see cref="Count"/> - <see cref="Index"/> the callback specified
        /// during construction is called as necessary.</remarks>
        public void Fill(byte[] buffer, int offset, int count)
        {
            var readCount = this.ReadFromBuffer(buffer, offset, count);
            offset += readCount;
            count -= readCount;

            if (count > 0)
            {
                if (count > this.Capacity)
                {
                    this.Compact();
                    StreamHelper.Fill(this.read, buffer, offset, count);
                    this.previousPosition += count;
                }
                else
                {
                    this.Fill(count);
                    System.Buffer.BlockCopy(this.GetBuffer(), this.Index, buffer, offset, count);
                    this.Index += count;
                }
            }
        }

        /// <summary>Asynchronously reads exactly <paramref name="count"/> bytes from the buffer.</summary>
        /// <exception cref="ArgumentException">The length of <paramref name="buffer"/> is less than
        /// <paramref name="offset"/> plus <paramref name="count"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> equals <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> and/or <paramref name="count"/>
        /// are negative.</exception>
        /// <exception cref="EndOfStreamException">The end of the stream has been reached before
        /// <paramref name="buffer"/> could be filled to <paramref name="count"/> bytes.</exception>
        /// <exception cref="InvalidOperationException">The <see cref="ReadBuffer"/> object was created by calling
        /// <see cref="ReadBuffer(ReadCallback, int)"/>.</exception>
        /// <remarks>If <paramref name="count"/> &gt; <see cref="Count"/> - <see cref="Index"/> the callback specified
        /// during construction is called as necessary.</remarks>
        public async Task FillAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var readCount = this.ReadFromBuffer(buffer, offset, count);
            offset += readCount;
            count -= readCount;

            if (count > 0)
            {
                if (count > this.Capacity)
                {
                    this.Compact();
                    await StreamHelper.FillAsync(readAsync, buffer, offset, count, cancellationToken).ConfigureAwait(false);
                    this.previousPosition += count;
                }
                else
                {
                    await FillAsync(count, cancellationToken).ConfigureAwait(false);
                    System.Buffer.BlockCopy(this.GetBuffer(), this.Index, buffer, offset, count);
                    this.Index += count;
                }
            }
        }

        /// <summary>Reads an UTF-8-encoded string from the buffer.</summary>
        /// <param name="count">The number of bytes the UTF-8 representation occupies in the buffer.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is negative.</exception>
        /// <exception cref="EndOfStreamException">The end of the stream has been reached before the
        /// the buffer could be filled to <paramref name="count"/> bytes.</exception>
        /// <exception cref="InvalidOperationException">The <see cref="ReadBuffer"/> object was created by calling
        /// <see cref="ReadBuffer(ReadAsyncCallback, int)"/>.</exception>
        /// <remarks>First calls <see cref="Fill(int)"/> to read all the necessary bytes into the buffer.</remarks>
        public string ReadUtf8(int count)
        {
            this.Fill(count);
            var result = Encoding.UTF8.GetString(this.GetBuffer(), this.Index, count);
            this.Index += count;
            return result;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private readonly ReadCallback read;
        private readonly ReadAsyncCallback readAsync;
        private long previousPosition;

        private int ReadFromBuffer(byte[] buffer, int offset, int count)
        {
            BufferHelper.AssertValidRange(buffer, "buffer", offset, "offset", count, "count");
            var readCount = Math.Min(count, this.Count - this.Index);
            System.Buffer.BlockCopy(this.GetBuffer(), this.Index, buffer, offset, readCount);
            this.Index += readCount;
            return readCount;
        }

        private void FillCore(int count)
        {
            this.EnsureSpace(count);

            while (count > this.Count - this.Index)
            {
                this.CheckAndAddToCount(this.read(this.GetBuffer(), this.Count, this.Capacity - this.Count));
            }
        }

        private async Task FillCoreAsync(int count, CancellationToken cancellationToken)
        {
            this.EnsureSpace(count);

            while (count > this.Count - this.Index)
            {
                var readCount = this.Capacity - this.Count;
                this.CheckAndAddToCount(
                    await readAsync(GetBuffer(), Count, readCount, cancellationToken).ConfigureAwait(false));
            }
        }

        private void EnsureSpace(int count)
        {
            if (count > this.Capacity - this.Index)
            {
                this.Compact();
                this.EnsureCapacity(count);
            }
        }

        private void CheckAndAddToCount(int readCount)
        {
            if (readCount == 0)
            {
                throw new EndOfStreamException("Unexpected end of stream.");
            }

            this.Count += readCount;
        }

        private void Compact()
        {
            var remainingCount = this.Count - this.Index;
            System.Buffer.BlockCopy(this.GetBuffer(), this.Index, this.GetBuffer(), 0, remainingCount);
            this.previousPosition += this.Index;
            this.Index = 0;
            this.Count = remainingCount;
        }
    }
}
