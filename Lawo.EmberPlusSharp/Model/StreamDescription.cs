////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2016 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System;

    /// <summary>Describes the format and the offset of a value in a stream.</summary>
    /// <threadsafety static="true" instance="false"/>
    public struct StreamDescription : IEquatable<StreamDescription>
    {
        private readonly StreamFormat format;
        private readonly int offset;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets the format.</summary>
        public StreamFormat Format
        {
            get { return this.format; }
        }

        /// <summary>Gets the offset in bytes.</summary>
        public int Offset
        {
            get { return this.offset; }
        }

        /// <inheritdoc/>
        public bool Equals(StreamDescription other)
        {
            return (this.format == other.format) && (this.offset == other.offset);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            var other = obj as StreamDescription?;
            return other.HasValue && this.Equals(other.Value);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine((int)this.format, this.offset);
        }

        /// <summary>Determines whether two specified instances of <see cref="StreamDescription"/> are equal.</summary>
        public static bool operator ==(StreamDescription left, StreamDescription right)
        {
            return left.Equals(right);
        }

        /// <summary>Determines whether two specified instances of <see cref="StreamDescription"/> are not equal.
        /// </summary>
        public static bool operator !=(StreamDescription left, StreamDescription right)
        {
            return !left.Equals(right);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal StreamDescription(StreamFormat format, int offset)
        {
            this.format = format;
            this.offset = offset;
        }
    }
}