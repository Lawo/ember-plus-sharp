////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
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
        /// <summary>Determines whether two specified instances of <see cref="StreamDescription"/> are equal.</summary>
        public static bool operator ==(StreamDescription left, StreamDescription right) => left.Equals(right);

        /// <summary>Determines whether two specified instances of <see cref="StreamDescription"/> are not equal.
        /// </summary>
        public static bool operator !=(StreamDescription left, StreamDescription right) => !left.Equals(right);

        /// <summary>Gets the format.</summary>
        public StreamFormat Format { get; }

        /// <summary>Gets the offset in bytes.</summary>
        public int Offset { get; }

        /// <inheritdoc/>
        public bool Equals(StreamDescription other) => (this.Format == other.Format) && (this.Offset == other.Offset);

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            var other = obj as StreamDescription?;
            return other.HasValue && this.Equals(other.Value);
        }

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine((int)this.Format, this.Offset);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal StreamDescription(StreamFormat format, int offset)
        {
            this.Format = format;
            this.Offset = offset;
        }
    }
}