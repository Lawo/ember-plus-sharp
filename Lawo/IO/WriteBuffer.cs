////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright 2008-2012 Andreas Huber Doenni, original from http://phuse.codeplex.com.
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.IO
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>References a method to be called when bytes need to written to a sink.</summary>
    /// <remarks>The referenced method must behave exactly like <see cref="Stream.Write(byte[], int, int)"/>.</remarks>
    public delegate void WriteCallback(byte[] buffer, int offset, int count);

    /// <summary>References a method to be called when bytes need to written to a sink asynchronously.</summary>
    /// <remarks>The referenced method must behave exactly like
    /// <see cref="Stream.WriteAsync(byte[], int, int, CancellationToken)"/>. Notably, calling code that needs to cancel
    /// a write operation will do so as follows:
    /// <list type="number">
    /// <item>It will call <see cref="CancellationTokenSource.Cancel()"/> on the object from which
    /// <paramref name="cancellationToken"/> originated and wait for an <see cref="OperationCanceledException"/> to be
    /// thrown.</item>
    /// <item>If no <see cref="OperationCanceledException"/> is thrown within say 500 milliseconds,
    /// <see cref="IDisposable.Dispose"/> is called on the object representing the sink and the caller then waits
    /// for either an <see cref="OperationCanceledException"/> or an <see cref="ObjectDisposedException"/>.</item>
    /// </list>
    /// The steps above ensure that cancellation will work correctly for APIs that do support
    /// <see cref="CancellationToken"/> as well as for those that don't (where the recommended practice is to simply
    /// call <see cref="IDisposable.Dispose"/> on the object representing the sink.</remarks>
    public delegate Task WriteAsyncCallback(byte[] buffer, int offset, int count, CancellationToken cancellationToken);

    /// <summary>Provides a thin wrapper for a buffer that is emptied by calling the provided callback.</summary>
    /// <remarks>
    /// <para>The fields in this class and its base are not encapsulated properly for performance reasons. See
    /// <see cref="ReadBuffer"/> for more information.</para>
    /// <para>A frequent use case for this class is when a not previously known number of bytes need to be written to
    /// a stream one by one. In this case the following code tends to be much faster than calling
    /// <see cref="Stream.WriteByte"/> for each byte:
    /// <code>
    /// void WriteToStream(Stream stream)
    /// {
    ///     var writeBuffer = new WriteBuffer(stream.Write, 1024);
    ///     bool done = false;
    ///
    ///     while (!done &amp;&amp; ((writeBuffer.Count &lt; writeBuffer.Capacity) || writeBuffer.Flush()))
    ///     {
    ///         // ..
    ///         byte theByte = 0x00; // Calculate the byte to append to the buffer
    ///         writeBuffer[writeBuffer.Count++] = theByte;
    ///
    ///         // Set done to true as soon as we're done...
    ///     }
    /// }
    /// </code>
    /// </para>
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    public sealed class WriteBuffer : Buffer
    {
        /// <summary>Initializes a new instance of the <see cref="WriteBuffer"/> class.</summary>
        /// <param name="write">The method that is called when the buffer needs to be emptied.</param>
        /// <param name="bufferSize">The size of the buffer in bytes.</param>
        /// <exception cref="ArgumentNullException"><paramref name="write"/> equals <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="bufferSize"/> is 0 or negative.</exception>
        public WriteBuffer(WriteCallback write, int bufferSize)
            : base(bufferSize)
        {
            this.write = write ?? throw new ArgumentNullException(nameof(write));
            this.writeAsync = (b, o, c, t) => ThrowInvalidAsyncOperationException();
        }

        /// <summary>Initializes a new instance of the <see cref="WriteBuffer"/> class.</summary>
        /// <param name="writeAsync">The asynchronous method that is called when the buffer needs to be emptied.</param>
        /// <param name="bufferSize">The size of the buffer in bytes.</param>
        /// <exception cref="ArgumentNullException"><paramref name="writeAsync"/> equals <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="bufferSize"/> is 0 or negative.</exception>
        public WriteBuffer(WriteAsyncCallback writeAsync, int bufferSize)
            : base(bufferSize)
        {
            this.writeAsync = writeAsync ?? throw new ArgumentNullException(nameof(writeAsync));
            this.write = (b, o, c) => ThrowInvalidSyncOperationException();
        }

        /// <summary>Gets or sets the number of bytes actually contained in the buffer.</summary>
        public int Count { get; set; }

        /// <summary>Empties the contents of the buffer by calling the callback specified at construction.</summary>
        /// <returns>The value <c>true</c>.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="WriteBuffer"/> object was created by calling
        /// <see cref="WriteBuffer(WriteAsyncCallback, int)"/>.</exception>
        /// <remarks>After calling this function, <see cref="Count"/> equals 0.</remarks>
        public bool Flush()
        {
            var count = this.Count;

            // Prevent call to this.write when buffer is empty so that there is no error when the underlying stream has
            // already been disposed.
            if (count > 0)
            {
                this.Count = 0; // Make sure that the buffer is empty even if write throws
                this.write(this.GetBuffer(), 0, count);
            }

            return true;
        }

        /// <summary>Asynchronously empties the contents of the buffer by calling the callback specified at
        /// construction.</summary>
        /// <returns>The value <c>true</c>.</returns>
        /// <exception cref="InvalidOperationException">The <see cref="WriteBuffer"/> object was created by calling
        /// <see cref="WriteBuffer(WriteCallback, int)"/>.</exception>
        /// <remarks>After calling this function, <see cref="Count"/> equals 0.</remarks>
        public async Task<bool> FlushAsync(CancellationToken cancellationToken)
        {
            var count = this.Count;
            this.Count = 0; // Make sure that the buffer is empty even if write throws
            await writeAsync(GetBuffer(), 0, count, cancellationToken).ConfigureAwait(false);
            return true;
        }

        /// <summary>Ensures that <paramref name="size"/> &lt;= (<see cref="Buffer.Capacity"/> -
        /// <see cref="Count"/>).</summary>
        /// <param name="size">The minimum number of bytes to reserve.</param>
        /// <exception cref="InvalidOperationException">The <see cref="WriteBuffer"/> object was created by calling
        /// <see cref="WriteBuffer(WriteAsyncCallback, int)"/>.</exception>
        /// <remarks>Performs the following steps:
        /// <list type="number">
        /// <item>If <paramref name="size"/> &gt; (<see cref="Buffer.Capacity"/> - <see cref="Count"/>), then
        /// calls <see cref="Flush"/>.</item>
        /// <item>If <paramref name="size"/> &gt; <see cref="Buffer.Capacity"/>, then enlarges the buffer such that it
        /// can hold at least <paramref name="size"/> bytes.</item>
        /// </list></remarks>
        public void Reserve(int size)
        {
            if (size > (this.Capacity - this.Count))
            {
                this.EnsureCapacity(size);
                this.Flush();
            }
        }

        /// <summary>Asynchronously ensures that <paramref name="size"/> &lt;= (<see cref="Buffer.Capacity"/> -
        /// <see cref="Count"/>).</summary>
        /// <param name="size">The minimum number of bytes to reserve.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <exception cref="InvalidOperationException">The <see cref="WriteBuffer"/> object was created by calling
        /// <see cref="WriteBuffer(WriteCallback, int)"/>.</exception>
        /// <remarks>Performs the following steps:
        /// <list type="number">
        /// <item>If <paramref name="size"/> &gt; (<see cref="Buffer.Capacity"/> - <see cref="Count"/>), then
        /// calls <see cref="FlushAsync"/>.</item>
        /// <item>If <paramref name="size"/> &gt; <see cref="Buffer.Capacity"/>, then enlarges the buffer such that it
        /// can hold at least <paramref name="size"/> bytes.</item>
        /// </list></remarks>
        public async Task ReserveAsync(int size, CancellationToken cancellationToken)
        {
            if (size > (this.Capacity - this.Count))
            {
                this.EnsureCapacity(size);
                await FlushAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>Writes the array segment identified by <paramref name="buffer"/>, <paramref name="offset"/> and
        /// <paramref name="count"/> to the buffer.</summary>
        /// <exception cref="ArgumentException">The length of <paramref name="buffer"/> is less than
        /// <paramref name="offset"/> plus <paramref name="count"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> equals <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> and/or <paramref name="count"/>
        /// are negative.</exception>
        /// <exception cref="InvalidOperationException">The <see cref="WriteBuffer"/> object was created by calling
        /// <see cref="WriteBuffer(WriteAsyncCallback, int)"/>.</exception>
        /// <remarks>The buffer is flushed as necessary.</remarks>
        public void Write(byte[] buffer, int offset, int count)
        {
            var written = this.WriteToBuffer(buffer, offset, count);
            count -= written;
            offset += written;

            if (count > 0)
            {
                this.Flush();

                if (count > this.Capacity)
                {
                    this.write(buffer, offset, count);
                }
                else
                {
                    System.Buffer.BlockCopy(buffer, offset, this.GetBuffer(), this.Count, count);
                    this.Count += count;
                }
            }
        }

        /// <summary>Asynchronously writes the array segment identified by <paramref name="buffer"/>,
        /// <paramref name="offset"/> and <paramref name="count"/> to the buffer.</summary>
        /// <exception cref="ArgumentException">The length of <paramref name="buffer"/> is less than
        /// <paramref name="offset"/> plus <paramref name="count"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> equals <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> and/or <paramref name="count"/>
        /// are negative.</exception>
        /// <exception cref="InvalidOperationException">The <see cref="WriteBuffer"/> object was created by calling
        /// <see cref="WriteBuffer(WriteCallback, int)"/>.</exception>
        /// <remarks>The buffer is flushed as necessary.</remarks>
        public async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var written = this.WriteToBuffer(buffer, offset, count);
            count -= written;
            offset += written;

            if (count > 0)
            {
                await FlushAsync(cancellationToken).ConfigureAwait(false);

                if (count > this.Capacity)
                {
                    await writeAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    System.Buffer.BlockCopy(buffer, offset, this.GetBuffer(), this.Count, count);
                    this.Count += count;
                }
            }
        }

        /// <summary>Writes the UTF-8 representation of <paramref name="value"/> to the buffer.</summary>
        /// <param name="value">The string to write to the buffer.</param>
        /// <param name="byteCount">The number of bytes the UTF-8 representation will need. Pass the return value of
        /// <see cref="Encoding.GetByteCount(string)"/>.</param>
        /// <exception cref="ArgumentException"><paramref name="byteCount"/> is less than the return value of
        /// <see cref="Encoding.GetByteCount(string)"/> called with <paramref name="value"/> as argument.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> equals <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">The <see cref="WriteBuffer"/> object was created by calling
        /// <see cref="WriteBuffer(WriteAsyncCallback, int)"/>.</exception>
        /// <remarks>If the remaining space in the buffer is too small to hold all bytes, the buffer is first flushed.
        /// If the buffer can still not hold all bytes, it is enlarged so that it can hold at least
        /// <paramref name="byteCount"/> bytes.</remarks>
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", Justification = "Official name for such counts in the BCL.")]
        public void WriteAsUtf8(string value, int byteCount)
        {
            AssertNotNull(value);
            this.Reserve(byteCount);
            this.Count += Encoding.UTF8.GetBytes(value, 0, value.Length, this.GetBuffer(), this.Count);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static void AssertNotNull(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
        }

        private readonly WriteCallback write;
        private readonly WriteAsyncCallback writeAsync;

        private int WriteToBuffer(byte[] buffer, int offset, int count)
        {
            BufferHelper.AssertValidRange(buffer, "buffer", offset, "offset", count, "count");
            var written = Math.Min(count, this.Capacity - this.Count);
            System.Buffer.BlockCopy(buffer, offset, this.GetBuffer(), this.Count, written);
            this.Count += written;
            return written;
        }
    }
}
