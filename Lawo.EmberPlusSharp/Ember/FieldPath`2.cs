////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Ember
{
    using System;

    internal struct FieldPath<TTypeId, TFieldId> : IEquatable<FieldPath<TTypeId, TFieldId>>
    {
        public bool Equals(FieldPath<TTypeId, TFieldId> other) =>
            this.field1.Equals(other.field1) && this.field2.Equals(other.field2) && this.field3.Equals(other.field3);

        public override int GetHashCode() =>
            HashCode.Combine(this.field1.GetHashCode(), this.field2.GetHashCode(), this.field3.GetHashCode());

        public override string ToString()
        {
            if (this.field1.HasValue)
            {
                return this.field1.ToString() + "." + this.field2.GetValueOrDefault().FieldId.ToString() + "." +
                    this.field3.GetValueOrDefault().FieldId.ToString();
            }
            else if (this.field2.HasValue)
            {
                return this.field2.ToString() + "." + this.field3.GetValueOrDefault().FieldId.ToString();
            }
            else
            {
                return this.field3.ToString();
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal static FieldPath<TTypeId, TFieldId> Append(
            FieldPath<TTypeId, TFieldId> path, Field<TTypeId, TFieldId> field)
        {
            if (path.field1.HasValue)
            {
                throw new ArgumentException("Cannot be appended.", nameof(path));
            }

            return new FieldPath<TTypeId, TFieldId>(path.field2, path.field3, field);
        }

        internal FieldPath(Field<TTypeId, TFieldId> field)
            : this(null, null, field)
        {
        }

        internal Field<TTypeId, TFieldId>? Tail => this.field3;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private readonly Field<TTypeId, TFieldId>? field1;
        private readonly Field<TTypeId, TFieldId>? field2;
        private readonly Field<TTypeId, TFieldId>? field3;

        private FieldPath(
            Field<TTypeId, TFieldId>? field1, Field<TTypeId, TFieldId>? field2, Field<TTypeId, TFieldId>? field3)
        {
            this.field1 = field1;
            this.field2 = field2;
            this.field3 = field3;
        }
    }
}
