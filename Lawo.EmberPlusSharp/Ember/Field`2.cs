////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Ember
{
    using System;

    internal struct Field<TTypeId, TFieldId> : IEquatable<Field<TTypeId, TFieldId>>
    {
        public bool Equals(Field<TTypeId, TFieldId> other) =>
            GenericCompare.Equals(this.TypeId, other.TypeId) && GenericCompare.Equals(this.FieldId, other.FieldId);

        public override int GetHashCode() => HashCode.Combine(this.TypeId.GetHashCode(), this.FieldId.GetHashCode());

        public override string ToString()
        {
            var typeIdString = this.TypeId.ToString();
            return typeIdString + (string.IsNullOrEmpty(typeIdString) ? string.Empty : ".") + this.FieldId.ToString();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal Field(TTypeId typeId, TFieldId fieldId)
        {
            this.TypeId = typeId;
            this.FieldId = fieldId;
        }

        internal TTypeId TypeId { get; }

        internal TFieldId FieldId { get; }
    }
}
