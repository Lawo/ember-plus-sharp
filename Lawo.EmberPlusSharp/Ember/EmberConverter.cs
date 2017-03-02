////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Ember
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Xml;

    using static System.Globalization.CultureInfo;

    /// <summary>Provides methods to convert EmBER to XML and XML to EmBER.</summary>
    /// <threadsafety static="true" instance="false"/>
    public sealed class EmberConverter : IEmberConverter
    {
        /// <summary>Initializes a new instance of the <see cref="EmberConverter"/> class.</summary>
        public EmberConverter()
            : this(new EmberTypeBag())
        {
        }

        /// <summary>Initializes a new instance of the <see cref="EmberConverter"/> class.</summary>
        /// <param name="types">A collection of <see cref="EmberType"/> instances to base the XML conversion on.</param>
        /// <exception cref="ArgumentNullException"><paramref name="types"/> equals <c>null</c>.</exception>
        public EmberConverter(EmberTypeBag types)
        {
            if (types == null)
            {
                throw new ArgumentNullException(nameof(types));
            }

            this.typeNames = types.TypeNames;
            this.fieldNames = types.FieldNames;
            this.innerNumbers = types.InnerNumbers;
            this.fieldIds = types.FieldIds;
        }

        /// <summary>Reads the EmBER-encoded data in <paramref name="buffer"/> and writes an equivalent XML
        /// representation with <paramref name="writer"/>.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> and/or <paramref name="writer"/> equal
        /// <c>null</c>.</exception>
        /// <exception cref="EmberException">The EmBER-encoded data is invalid, see <see cref="Exception.Message"/> for
        /// details.</exception>
        public void ToXml(byte[] buffer, XmlWriter writer)
        {
            using (var stream = new MemoryStream(buffer))
            using (var reader = new EmberReader(stream))
            {
                this.ToXml(reader, writer);
            }
        }

        /// <summary>Reads EmBER data with <paramref name="reader"/> and writes an equivalent XML representation with
        /// <paramref name="writer"/>.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="reader"/> and/or <paramref name="writer"/> equal
        /// <c>null</c>.</exception>
        /// <exception cref="EmberException">The EmBER-encoded data is invalid, see <see cref="Exception.Message"/> for
        /// details.</exception>
        public void ToXml(EmberReader reader, XmlWriter writer)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            this.ToXmlCore(reader, writer, default(FieldPath<int, EmberId>), EmberGlobal.InnerNumber);
        }

        /// <summary>Reads XML with <paramref name="reader"/> and returns the equivalent EmBER representation.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="reader"/> equals <c>null</c>.</exception>
        /// <exception cref="XmlException">The XML is invalid, see <see cref="Exception.Message"/> for details.
        /// </exception>
        public byte[] FromXml(XmlReader reader)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new EmberWriter(stream))
                {
                    this.FromXml(reader, writer);
                }

                return stream.ToArray();
            }
        }

        /// <summary>Reads XML with <paramref name="reader"/> and writes the equivalent EmBER representation with
        /// <paramref name="writer"/>.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="reader"/> and/or <paramref name="writer"/> equal
        /// <c>null</c>.</exception>
        /// <exception cref="XmlException">The XML is invalid, see <see cref="Exception.Message"/> for details.
        /// </exception>
        public void FromXml(XmlReader reader, EmberWriter writer)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            this.FromXmlCore(reader, writer, default(FieldPath<string, string>), EmberGlobal.Name);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static FieldPath<int, EmberId> Combine(FieldPath<int, EmberId> path, Field<int, EmberId> field)
        {
            if ((field.TypeId == BerSequence.InnerNumber) || (field.TypeId == BerSet.InnerNumber))
            {
                return FieldPath<int, EmberId>.Append(path, field);
            }
            else
            {
                return new FieldPath<int, EmberId>(field);
            }
        }

        private static FieldPath<string, string> Combine(FieldPath<string, string> path, Field<string, string> field)
        {
            if ((field.TypeId == BerSequence.Name) || (field.TypeId == BerSet.Name))
            {
                return FieldPath<string, string>.Append(path, field);
            }
            else
            {
                return new FieldPath<string, string>(field);
            }
        }

        private static void WriteValue<T>(
            XmlWriter writer, string fieldName, string type, T value, Action<XmlWriter, T> writeValue)
        {
            writer.WriteStartElement(fieldName);
            WriteType(writer, type);
            writeValue(writer, value);
            writer.WriteEndElement();
        }

        private static void WriteStartContainer(XmlWriter writer, string fieldName, string type)
        {
            writer.WriteStartElement(fieldName);
            WriteType(writer, type);
        }

        private static void WriteType(XmlWriter writer, string type)
        {
            writer.WriteStartAttribute("type");
            writer.WriteString(type);
            writer.WriteEndAttribute();
        }

        private static bool ReadNext(XmlReader reader) => reader.Read() && (reader.MoveToContent() != XmlNodeType.None);

        private static TValue ReadValue<TValue>(
            XmlReader reader, Func<XmlReader, TValue> read, TValue emptyValue = default(TValue))
        {
            if (reader.IsEmptyElement)
            {
                if (emptyValue.Equals(default(TValue)))
                {
                    const string Format = "Unexpected empty element for a field of type {0}.";
                    throw new XmlException(string.Format(InvariantCulture, Format, typeof(TValue).Name));
                }
                else
                {
                    return emptyValue;
                }
            }

            reader.ReadStartElement();
            return read(reader);
        }

        private static void WriteOctetstring(XmlReader reader, EmberWriter writer, EmberId fieldId)
        {
            var buffer = new byte[1024];

            using (var stream = new MemoryStream())
            {
                var read = ReadValue(reader, r => (int?)r.ReadContentAsBinHex(buffer, 0, buffer.Length), 0);

                while (read > 0)
                {
                    stream.Write(buffer, 0, read.Value);
                    read = reader.ReadContentAsBinHex(buffer, 0, buffer.Length);
                }

                writer.WriteValue(fieldId, stream.ToArray());
            }
        }

        private static void WriteRelativeObjectIdentifier(XmlReader reader, EmberWriter writer, EmberId fieldId)
        {
            var pathElements = ReadValue(reader, r => r.ReadContentAsString()).Split('.');
            var value = (pathElements.Length == 1) && string.IsNullOrEmpty(pathElements[0]) ?
                new int[0] : pathElements.Select(s => int.Parse(s, InvariantCulture)).ToArray();
            writer.WriteValue(fieldId, value);
        }

        private static string GetFallbackName(int innerNumber) => EmberId.FromInnerNumber(innerNumber).ToString();

        private readonly Dictionary<int, string> typeNames;
        private readonly Dictionary<FieldPath<int, EmberId>, string> fieldNames;
        private readonly Dictionary<string, int> innerNumbers;
        private readonly Dictionary<FieldPath<string, string>, EmberId> fieldIds;

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "There's no meaningful way to reduce the complexity.")]
        private void ToXmlCore(
            EmberReader reader, XmlWriter writer, FieldPath<int, EmberId> previousPath, int currentType)
        {
            while (reader.Read())
            {
                var nextType = reader.InnerNumber;

                if (nextType == InnerNumber.EndContainer)
                {
                    writer.WriteEndElement();
                    return;
                }

                var currentPath = Combine(previousPath, new Field<int, EmberId>(currentType, reader.OuterId));
                var fieldName = this.GetFieldName(currentPath);

                switch (nextType)
                {
                    case BerBoolean.InnerNumber:
                        var boolean = reader.ReadContentsAsBoolean();
                        WriteValue(writer, fieldName, BerBoolean.Name, boolean, (w, o) => w.WriteValue(o));
                        break;
                    case BerInteger.InnerNumber:
                        var integer = reader.ReadContentsAsInt64();
                        WriteValue(writer, fieldName, BerInteger.Name, integer, (w, o) => w.WriteValue(o));
                        break;
                    case BerReal.InnerNumber:
                        WriteValue(
                            writer, fieldName, BerReal.Name, reader.ReadContentsAsDouble(), (w, o) => w.WriteValue(o));
                        break;
                    case BerUtf8String.InnerNumber:
                        string str;

                        try
                        {
                            str = XmlConvert.VerifyXmlChars(reader.ReadContentsAsString());
                        }
                        catch (XmlException)
                        {
                            str = "*** CONTAINS INVALID XML CHARACTERS ***";
                        }

                        switch (currentPath.ToString())
                        {
                            case "1073741825.C-1.C-6":
                            case "1073741825.C-1.C-7":
                            case "1073741825.C-1.C-17":
                            case "1073741833.C-1.C-6":
                            case "1073741833.C-1.C-7":
                            case "1073741833.C-1.C-17":
                            case "1073741827.C-1.C-4":
                            case "1073741834.C-1.C-4":
                            case "1073741837.C-1.C-11":
                            case "1073741841.C-1.C-11":
                                str = str.Replace("\n", Environment.NewLine);
                                break;
                            default:
                                // Intentionally do nothing
                                break;
                        }

                        WriteValue(writer, fieldName, BerUtf8String.Name, str, (w, o) => w.WriteValue(o));
                        break;
                    case BerOctetstring.InnerNumber:
                        var bytes = reader.ReadContentsAsByteArray();
                        WriteValue(
                            writer, fieldName, BerOctetstring.Name, bytes, (w, o) => w.WriteBinHex(o, 0, o.Length));
                        break;
                    case BerRelativeObjectIdentifier.InnerNumber:
                        var intOid = reader.ReadContentsAsInt32Array();
                        var oid = string.Join(".", intOid.Select(e => e.ToString(InvariantCulture)));
                        WriteValue(writer, fieldName, BerRelativeObjectIdentifier.Name, oid, (w, o) => w.WriteValue(o));
                        break;
                    case BerSequence.InnerNumber:
                        WriteStartContainer(writer, fieldName, BerSequence.Name);
                        this.ToXmlCore(reader, writer, currentPath, nextType);
                        break;
                    case BerSet.InnerNumber:
                        WriteStartContainer(writer, fieldName, BerSet.Name);
                        this.ToXmlCore(reader, writer, currentPath, nextType);
                        break;
                    default:
                        WriteStartContainer(writer, fieldName, this.GetTypeName(nextType));
                        this.ToXmlCore(reader, writer, currentPath, nextType);
                        break;
                }
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "There's no meaningful way to reduce the complexity.")]
        private void FromXmlCore(
            XmlReader reader, EmberWriter writer, FieldPath<string, string> previousPath, string currentType)
        {
            if (reader.IsEmptyElement)
            {
                writer.WriteEndContainer();
                return;
            }

            while (ReadNext(reader))
            {
                if (reader.NodeType == XmlNodeType.EndElement)
                {
                    writer.WriteEndContainer();
                    return;
                }

                if (reader.NodeType != XmlNodeType.Element)
                {
                    const string Format = "Unexpected Node Type: Encountered {0} while looking for {1}.";
                    throw new XmlException(
                        string.Format(InvariantCulture, Format, reader.NodeType, XmlNodeType.Element));
                }

                var currentPath = Combine(previousPath, new Field<string, string>(currentType, reader.Name));
                var fieldId = this.GetFieldId(currentPath);

                if (reader.AttributeCount != 1)
                {
                    throw new XmlException(
                        "Unexpected Attribute Count: Each element must have exactly one type attribute.");
                }

                var nextType = reader.GetAttribute(0);

                switch (nextType)
                {
                    case BerBoolean.Name:
                        writer.WriteValue(fieldId, ReadValue(reader, r => r.ReadContentAsBoolean()));
                        break;
                    case BerInteger.Name:
                        writer.WriteValue(fieldId, ReadValue(reader, r => r.ReadContentAsLong()));
                        break;
                    case BerOctetstring.Name:
                        WriteOctetstring(reader, writer, fieldId);
                        break;
                    case BerReal.Name:
                        writer.WriteValue(fieldId, ReadValue(reader, r => r.ReadContentAsDouble()));
                        break;
                    case BerUtf8String.Name:
                        var value = ReadValue(reader, r => r.ReadContentAsString(), string.Empty);

                        switch (currentPath.ToString())
                        {
                            case "Parameter.contents.formula":
                            case "Parameter.contents.enumeration":
                            case "Parameter.contents.schemaIdentifiers":
                            case "QualifiedParameter.contents.formula":
                            case "QualifiedParameter.contents.enumeration":
                            case "QualifiedParameter.contents.schemaIdentifiers":
                            case "Node.contents.schemaIdentifiers":
                            case "QualifiedNode.contents.schemaIdentifiers":
                            case "Matrix.contents.schemaIdentifiers":
                            case "QualifiedMatrix.contents.schemaIdentifiers":
                                value = value.Replace(Environment.NewLine, "\n");
                                break;
                            default:
                                // Intentionally do nothing
                                break;
                        }

                        writer.WriteValue(fieldId, value);
                        break;
                    case BerRelativeObjectIdentifier.Name:
                        WriteRelativeObjectIdentifier(reader, writer, fieldId);
                        break;
                    case BerSequence.Name:
                        writer.WriteStartSequence(fieldId);
                        this.FromXmlCore(reader, writer, currentPath, nextType);
                        break;
                    case BerSet.Name:
                        writer.WriteStartSet(fieldId);
                        this.FromXmlCore(reader, writer, currentPath, nextType);
                        break;
                    default:
                        writer.WriteStartApplicationDefinedType(fieldId, this.GetInnerNumber(nextType));
                        this.FromXmlCore(reader, writer, currentPath, nextType);
                        break;
                }
            }
        }

        private string GetTypeName(int innerNumber)
        {
            string name;
            return this.typeNames.TryGetValue(innerNumber, out name) ? name : GetFallbackName(innerNumber);
        }

        private int GetInnerNumber(string type)
        {
            int innerNumber;

            if (this.innerNumbers.TryGetValue(type, out innerNumber))
            {
                return innerNumber;
            }

            EmberId id;
            int? innerNumberCandidate;

            if (EmberId.TryParse(type, out id) && (innerNumberCandidate = id.ToInnerNumber()).HasValue)
            {
                return innerNumberCandidate.Value;
            }

            throw new XmlException(string.Format(InvariantCulture, "Unknown type: {0}.", type));
        }

        private string GetFieldName(FieldPath<int, EmberId> fieldPath)
        {
            string name;

            if (this.fieldNames.TryGetValue(fieldPath, out name))
            {
                return name;
            }

            return fieldPath.Tail.GetValueOrDefault().FieldId.ToString();
        }

        private EmberId GetFieldId(FieldPath<string, string> fieldPath)
        {
            EmberId emberId;

            if (this.fieldIds.TryGetValue(fieldPath, out emberId) ||
                EmberId.TryParse(fieldPath.Tail.GetValueOrDefault().FieldId, out emberId))
            {
                return emberId;
            }

            throw new XmlException(string.Format(InvariantCulture, "Unknown field path: {0}.", fieldPath));
        }
    }
}
