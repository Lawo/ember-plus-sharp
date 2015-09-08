////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Ember
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>Represents an unordered collection of <see cref="EmberType"/> instances.</summary>
    /// <remarks>
    /// <para>There's usually exactly one singleton <see cref="EmberTypeBag"/> instance for each DTD, see e.g.
    /// <see cref="Glow.GlowTypes"/>.</para>
    /// <para><b>Thread Safety</b>: Any public static members of this type are thread safe. Any instance members are not
    /// guaranteed to be thread safe.</para>
    /// </remarks>
    public sealed class EmberTypeBag
    {
        private const string InnerNumberFieldName = "InnerNumber";
        private const string NameFieldName = "Name";
        private const string OuterIdFieldName = "OuterId";

        private static readonly EmberType[] BerTypes =
        {
            typeof(BerBoolean),
            typeof(BerInteger),
            typeof(BerOctetstring),
            typeof(BerReal),
            typeof(BerUtf8String),
            typeof(BerRelativeObjectIdentifier),
            typeof(BerSequence),
            typeof(BerSet)
        };

        private readonly Dictionary<int, string> typeNames;
        private readonly Dictionary<FieldPath<int, EmberId>, string> fieldNames;
        private readonly Dictionary<string, int> innerNumbers;
        private readonly Dictionary<FieldPath<string, string>, EmberId> fieldIds;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Initializes a new instance of the <see cref="EmberTypeBag"/> class.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="types"/> equals <c>null</c>.</exception>
        /// <remarks>Besides <paramref name="types"/> the resulting collection will also contain
        /// <see cref="BerBoolean"/>, <see cref="BerInteger"/>, <see cref="BerOctetstring"/>, <see cref="BerReal"/>,
        /// <see cref="BerUtf8String"/>, <see cref="BerRelativeObjectIdentifier"/>, <see cref="BerSequence"/> and
        /// <see cref="BerSet"/>.</remarks>
        public EmberTypeBag(params EmberType[] types)
        {
            if (types == null)
            {
                throw new ArgumentNullException("types");
            }

            var allTypes = BerTypes.Concat(types).ToArray();

            this.typeNames = new Dictionary<int, string>(allTypes.Length);
            this.fieldNames = new Dictionary<FieldPath<int, EmberId>, string>(allTypes.Length * 3);
            this.innerNumbers = new Dictionary<string, int>(allTypes.Length);
            this.fieldIds = new Dictionary<FieldPath<string, string>, EmberId>(allTypes.Length * 3);

            foreach (var type in allTypes)
            {
                // TODO: Check for errors and report them with exceptions.
                var typeInfo = type.Type.GetTypeInfo();
                var innerNo = (int)typeInfo.GetDeclaredField(InnerNumberFieldName).GetValue(null);
                var nameField = typeInfo.GetDeclaredField(NameFieldName);

                if (nameField != null)
                {
                    var name = (string)nameField.GetValue(null);
                    this.typeNames.Add(innerNo, name);
                    this.innerNumbers.Add(name, innerNo);
                }
            }

            foreach (var type in allTypes)
            {
                var typeInfo = type.Type.GetTypeInfo();
                var innerNumber = (int)typeInfo.GetDeclaredField(InnerNumberFieldName).GetValue(null);
                var innerTypeName = this.typeNames[innerNumber];

                var outerFieldIds = GetOuterFieldsIds(type.OuterFields);
                var outerFieldNames = GetOuterFieldsNames(this.typeNames, type.OuterFields);

                foreach (var nestedTypeInfo in typeInfo.DeclaredNestedTypes)
                {
                    var innerFieldId = (EmberId)nestedTypeInfo.GetDeclaredField(OuterIdFieldName).GetValue(null);
                    var innerFieldIds = new Field<int, EmberId>(innerNumber, innerFieldId);
                    var innerFieldName = (string)nestedTypeInfo.GetDeclaredField(NameFieldName).GetValue(null);
                    var innerFieldNames = new Field<string, string>(innerTypeName, innerFieldName);
                    this.fieldNames.Add(FieldPath<int, EmberId>.Append(outerFieldIds, innerFieldIds), innerFieldName);
                    this.fieldIds.Add(FieldPath<string, string>.Append(outerFieldNames, innerFieldNames), innerFieldId);
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal Dictionary<int, string> TypeNames
        {
            get { return this.typeNames; }
        }

        internal Dictionary<FieldPath<int, EmberId>, string> FieldNames
        {
            get { return this.fieldNames; }
        }

        internal Dictionary<string, int> InnerNumbers
        {
            get { return this.innerNumbers; }
        }

        internal Dictionary<FieldPath<string, string>, EmberId> FieldIds
        {
            get { return this.fieldIds; }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static FieldPath<int, EmberId> GetOuterFieldsIds(IEnumerable<Type> outerFields)
        {
            return outerFields.Aggregate(
                new FieldPath<int, EmberId>(), (p, f) => FieldPath<int, EmberId>.Append(p, GetFieldIds(f)));
        }

        private static Field<int, EmberId> GetFieldIds(Type outerField)
        {
            return new Field<int, EmberId>(
                (int)outerField.DeclaringType.GetTypeInfo().GetDeclaredField(InnerNumberFieldName).GetValue(null),
                (EmberId)outerField.GetTypeInfo().GetDeclaredField(OuterIdFieldName).GetValue(null));
        }

        private static FieldPath<string, string> GetOuterFieldsNames(
            Dictionary<int, string> typeNames, IEnumerable<Type> outerFields)
        {
            return outerFields.Aggregate(
                new FieldPath<string, string>(), (p, f) => FieldPath<string, string>.Append(p, GetFieldNames(typeNames, f)));
        }

        private static Field<string, string> GetFieldNames(Dictionary<int, string> typeNames, Type outerField)
        {
            var typeName = typeNames[(int)outerField.DeclaringType.GetTypeInfo().GetDeclaredField(
                InnerNumberFieldName).GetValue(null)];
            return new Field<string, string>(
                typeName, (string)outerField.GetTypeInfo().GetDeclaredField(NameFieldName).GetValue(null));
        }
    }
}
