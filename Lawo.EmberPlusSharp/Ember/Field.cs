////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2016 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Ember
{
    using System;

    internal struct Field<TTypeId, TFieldId> : IEquatable<Field<TTypeId, TFieldId>>
    {
        public bool Equals(Field<TTypeId, TFieldId> other)
        {
            return GenericCompare.Equals(this.typeId, other.typeId) &&
                GenericCompare.Equals(this.fieldId, other.fieldId);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.typeId.GetHashCode(), this.fieldId.GetHashCode());
        }

        public override string ToString()
        {
            var typeIdString = this.typeId.ToString();
            return typeIdString + (string.IsNullOrEmpty(typeIdString) ? string.Empty : ".") + this.fieldId.ToString();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal Field(TTypeId typeId, TFieldId fieldId)
        {
            this.typeId = typeId;
            this.fieldId = fieldId;
        }

        internal TTypeId TypeId
        {
            get { return this.typeId; }
        }

        internal TFieldId FieldId
        {
            get { return this.fieldId; }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private readonly TTypeId typeId;
        private readonly TFieldId fieldId;
    }
}
