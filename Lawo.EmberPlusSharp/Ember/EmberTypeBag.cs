////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Ember
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>Represents an unordered collection of <see cref="EmberType"/> instances.</summary>
    /// <remarks>There's usually exactly one singleton <see cref="EmberTypeBag"/> instance for each DTD, see e.g.
    /// <see cref="Glow.GlowTypes"/>.</remarks>
    /// <threadsafety static="true" instance="false"/>
    public sealed class EmberTypeBag
    {
        /// <summary>Initializes a new instance of the <see cref="EmberTypeBag"/> class.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="types"/> equals <c>null</c>.</exception>
        /// <remarks>Besides <paramref name="types"/> the resulting collection will also contain
        /// <see cref="BerBoolean"/>, <see cref="BerInteger"/>, <see cref="BerOctetstring"/>, <see cref="BerReal"/>,
        /// <see cref="BerUtf8String"/>, <see cref="BerRelativeObjectIdentifier"/>, <see cref="BerSequence"/> and
        /// <see cref="BerSet"/>.</remarks>
        public EmberTypeBag(params EmberType[] types)
        {
            var allTypes = BerTypes.Concat(types ?? throw new ArgumentNullException(nameof(types))).ToArray();
            this.TypeNames = new Dictionary<int, string>(allTypes.Length);
            this.FieldNames = new Dictionary<FieldPath<int, EmberId>, string>(allTypes.Length * 3);
            this.InnerNumbers = new Dictionary<string, int>(allTypes.Length);
            this.FieldIds = new Dictionary<FieldPath<string, string>, EmberId>(allTypes.Length * 3);

            foreach (var type in allTypes)
            {
                // TODO: Check for errors and report them with exceptions.
                var typeInfo = type.Type.GetTypeInfo();
                var innerNo = (int)typeInfo.GetDeclaredField(InnerNumberFieldName).GetValue(null);
                var nameField = typeInfo.GetDeclaredField(NameFieldName);

                if (nameField != null)
                {
                    var name = (string)nameField.GetValue(null);
                    this.TypeNames.Add(innerNo, name);
                    this.InnerNumbers.Add(name, innerNo);
                }
            }

            foreach (var type in allTypes)
            {
                var typeInfo = type.Type.GetTypeInfo();
                var innerNumber = (int)typeInfo.GetDeclaredField(InnerNumberFieldName).GetValue(null);
                var innerTypeName = this.TypeNames[innerNumber];

                var outerFieldIds = GetOuterFieldsIds(type.OuterFields);
                var outerFieldNames = GetOuterFieldsNames(this.TypeNames, type.OuterFields);

                foreach (var nestedTypeInfo in typeInfo.DeclaredNestedTypes)
                {
                    var innerFieldId = (EmberId)nestedTypeInfo.GetDeclaredField(OuterIdFieldName).GetValue(null);
                    var innerFieldIds = new Field<int, EmberId>(innerNumber, innerFieldId);
                    var innerFieldName = (string)nestedTypeInfo.GetDeclaredField(NameFieldName).GetValue(null);
                    var innerFieldNames = new Field<string, string>(innerTypeName, innerFieldName);
                    this.FieldNames.Add(FieldPath<int, EmberId>.Append(outerFieldIds, innerFieldIds), innerFieldName);
                    this.FieldIds.Add(FieldPath<string, string>.Append(outerFieldNames, innerFieldNames), innerFieldId);
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal Dictionary<int, string> TypeNames { get; }

        internal Dictionary<FieldPath<int, EmberId>, string> FieldNames { get; }

        internal Dictionary<string, int> InnerNumbers { get; }

        internal Dictionary<FieldPath<string, string>, EmberId> FieldIds { get; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

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

        private static FieldPath<int, EmberId> GetOuterFieldsIds(IEnumerable<Type> outerFields) =>
            outerFields.Aggregate(
                default(FieldPath<int, EmberId>), (p, f) => FieldPath<int, EmberId>.Append(p, GetFieldIds(f)));

        private static Field<int, EmberId> GetFieldIds(Type outerField) =>
            new Field<int, EmberId>(
                (int)outerField.DeclaringType.GetTypeInfo().GetDeclaredField(InnerNumberFieldName).GetValue(null),
                (EmberId)outerField.GetTypeInfo().GetDeclaredField(OuterIdFieldName).GetValue(null));

        private static FieldPath<string, string> GetOuterFieldsNames(
            Dictionary<int, string> typeNames, IEnumerable<Type> outerFields)
        {
            return outerFields.Aggregate(
                default(FieldPath<string, string>), (p, f) => FieldPath<string, string>.Append(p, GetFieldNames(typeNames, f)));
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
