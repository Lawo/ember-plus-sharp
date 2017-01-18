////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright 2008-2012 Andreas Huber Doenni, original from http://phuse.codeplex.com.
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.IO
{
    using System;
    using System.Threading.Tasks;

    /// <summary>Implements the common members of <see cref="ReadBuffer"/> and <see cref="WriteBuffer"/>.</summary>
    /// <threadsafety static="true" instance="false"/>
    public abstract class Buffer
    {
        /// <summary>Gets the number of bytes the buffer can contain.</summary>
        public int Capacity => this.buffer.Length;

        /// <summary>Gets or sets the byte in the buffer at <paramref name="index"/>.</summary>
        /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> >= <see cref="Capacity"/>.</exception>
        public byte this[int index]
        {
            get { return this.buffer[index]; }
            set { this.buffer[index] = value; }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal static Task<int> ThrowInvalidAsyncOperationException()
        {
            throw new InvalidOperationException("Call to async operation on sync buffer.");
        }

        internal static int ThrowInvalidSyncOperationException()
        {
            throw new InvalidOperationException("Call to sync operation on async buffer.");
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Initializes a new instance of the <see cref="Buffer"/> class.</summary>
        /// <param name="bufferSize">The size of the buffer in bytes.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="bufferSize"/> is 0 or negative.</exception>
        protected Buffer(int bufferSize)
        {
            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize), ExceptionMessages.PositiveNumberRequired);
            }

            this.buffer = new byte[bufferSize];
        }

        /// <summary>Gets a reference to the internal <see cref="byte"/> array.</summary>
        protected byte[] GetBuffer()
        {
            return this.buffer;
        }

        /// <summary>Ensures that <see cref="Capacity"/> >= <paramref name="size"/>.</summary>
        protected void EnsureCapacity(int size)
        {
            if (size > this.buffer.Length)
            {
                Array.Resize(ref this.buffer, size);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private byte[] buffer;
    }
}
