////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Text;

    using Ember;
    using Glow;

    /// <summary>Represents a root object containing a number of static elements accessible through
    /// <see cref="Consumer{T}.Root">Consumer&lt;TRoot&gt;.Root</see>.</summary>
    /// <typeparam name="TMostDerived">The most-derived subtype of this class.</typeparam>
    /// <remarks>
    /// <para><typeparamref name="TMostDerived"/> must contain a property with a getter and a setter for each root
    /// element. The property getters and setters can have any accessibility. The name of each property must be equal to
    /// the identifier of the corresponding element, or carry an <see cref="ElementAttribute"/> to which the identifier
    /// is passed.</para>
    /// <para>The type of each <typeparamref name="TMostDerived"/> property must be of one of the following:
    /// <list type="bullet">
    /// <item><see cref="BooleanParameter"/>.</item>
    /// <item><see cref="IntegerParameter"/>.</item>
    /// <item><see cref="OctetstringParameter"/>.</item>
    /// <item><see cref="RealParameter"/>.</item>
    /// <item><see cref="StringParameter"/>.</item>
    /// <item>A <see cref="FieldNode{TMostDerived}"/> subtype.</item>
    /// <item><see cref="CollectionNode{TElement}"/>.</item>
    /// </list></para>
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance", Justification = "Fewer levels of inheritance would lead to more code duplication.")]
    public abstract class Root<TMostDerived> : FieldNode<TMostDerived>, IParent
        where TMostDerived : Root<TMostDerived>
    {
        /// <inheritdoc/>
        void IParent.SetHasChanges()
        {
            if (!this.HasChanges)
            {
                this.HasChanges = true;
                this.RaiseSendRequired();
            }
        }

        /// <inheritdoc/>
        void IParent.AppendPath(StringBuilder builder) => this.AppendPath(builder);

        internal event EventHandler<EventArgs> SendRequired;

        internal void Read(
            EmberReader reader,
            IDictionary<int, IInvocationResult> pendingInvocations,
            IReadOnlyDictionary<int, IEnumerable<IStreamedParameter>> streamedParameters)
        {
            reader.ReadAndAssertOuter(GlowGlobal.Root.OuterId);

            switch (reader.InnerNumber)
            {
                case GlowRootElementCollection.InnerNumber:
                    this.ReadChildren(reader);
                    break;
                case GlowInvocationResult.InnerNumber:
                    ReadInvocationResult(reader, pendingInvocations);
                    break;
                case GlowStreamCollection.InnerNumber:
                    ReadStreamCollection(reader, streamedParameters);
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        internal sealed override bool GetIsRoot() => true;

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Method is not public, CA bug?")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "RootElement", Justification = "Official Glow name.")]
        internal sealed override bool ReadChildrenCore(EmberReader reader)
        {
            var isEmpty = true;

            while (reader.Read() && (reader.InnerNumber != InnerNumber.EndContainer))
            {
                switch (reader.InnerNumber)
                {
                    case GlowParameter.InnerNumber:
                        isEmpty = false;
                        this.ReadChild(reader, ElementType.Parameter);
                        break;
                    case GlowNode.InnerNumber:
                        isEmpty = false;
                        this.ReadChild(reader, ElementType.Node);
                        break;
                    case GlowFunction.InnerNumber:
                        isEmpty = false;
                        this.ReadChild(reader, ElementType.Function);
                        break;
                    case GlowMatrix.InnerNumber:
                        isEmpty = false;
                        this.ReadChild(reader, ElementType.Matrix);
                        break;
                    case GlowQualifiedParameter.InnerNumber:
                        isEmpty = false;
                        this.ReadQualifiedChild(reader, ElementType.Parameter);
                        break;
                    case GlowQualifiedNode.InnerNumber:
                        isEmpty = false;
                        this.ReadQualifiedChild(reader, ElementType.Node);
                        break;
                    case GlowQualifiedFunction.InnerNumber:
                        isEmpty = false;
                        this.ReadQualifiedChild(reader, ElementType.Function);
                        break;
                    case GlowQualifiedMatrix.InnerNumber:
                        isEmpty = false;
                        this.ReadQualifiedChild(reader, ElementType.Matrix);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            return isEmpty;
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Method is not public, CA bug?")]
        internal sealed override bool WriteRequest(EmberWriter writer, IStreamedParameterCollection streamedParameters)
        {
            writer.WriteStartApplicationDefinedType(GlowGlobal.Root.OuterId, GlowRootElementCollection.InnerNumber);
            var result = this.WriteCommandCollection(writer, streamedParameters);
            writer.WriteEndContainer();
            return result;
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Method is not public, CA bug?")]
        internal sealed override void WriteChanges(EmberWriter writer, IInvocationCollection pendingInvocations)
        {
            if (this.HasChanges)
            {
                writer.WriteStartApplicationDefinedType(GlowGlobal.Root.OuterId, GlowRootElementCollection.InnerNumber);
                this.WriteChangesCollection(writer, pendingInvocations);
                writer.WriteEndContainer();
                this.HasChanges = false;
            }
        }

        internal sealed override void ResetRetrievalState()
        {
            this.RetrievalState = RetrievalState.None;
            this.RaiseSendRequired();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Initializes a new instance of the <see cref="Root{TMostDerived}"/> class.</summary>
        /// <remarks>
        /// <para>Objects of subtypes are not created by client code directly but indirectly when a
        /// <see cref="Consumer{T}"/> object is created.</para>
        /// </remarks>
        protected Root()
        {
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private static void ReadInvocationResult(
            EmberReader reader, IDictionary<int, IInvocationResult> pendingInvocations)
        {
            var success = true;
            IInvocationResult result = null;

            while (reader.Read() && (reader.InnerNumber != InnerNumber.EndContainer))
            {
                switch (reader.GetContextSpecificOuterNumber())
                {
                    case GlowInvocationResult.InvocationId.OuterNumber:
                        var invocationId = reader.AssertAndReadContentsAsInt32();
                        pendingInvocations.TryGetValue(invocationId, out result);
                        pendingInvocations.Remove(invocationId);
                        break;
                    case GlowInvocationResult.Success.OuterNumber:
                        success = reader.ReadContentsAsBoolean();
                        break;
                    case GlowInvocationResult.Result.OuterNumber:
                        if (result != null)
                        {
                            result.Read(reader);
                        }
                        else
                        {
                            reader.Skip();
                        }

                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            result?.Publish(success);
        }

        private static void ReadStreamCollection(
            EmberReader reader, IReadOnlyDictionary<int, IEnumerable<IStreamedParameter>> streamedParameters)
        {
            while (reader.Read() && (reader.InnerNumber != InnerNumber.EndContainer))
            {
                switch (reader.GetContextSpecificOuterNumber())
                {
                    case GlowStreamCollection.StreamEntry.OuterNumber:
                        ReadStreamEntry(reader, streamedParameters);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }
        }

        private static void ReadStreamEntry(
            EmberReader reader, IReadOnlyDictionary<int, IEnumerable<IStreamedParameter>> streamedParameters)
        {
            reader.AssertInnerNumber(GlowStreamEntry.InnerNumber);
            int? identifier = null;
            object rawValue = null;

            while (reader.Read() && (reader.InnerNumber != InnerNumber.EndContainer))
            {
                switch (reader.GetContextSpecificOuterNumber())
                {
                    case GlowStreamEntry.StreamIdentifier.OuterNumber:
                        identifier = reader.AssertAndReadContentsAsInt32();
                        break;
                    case GlowStreamEntry.StreamValue.OuterNumber:
                        rawValue = reader.ReadContentsAsObject();
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            IEnumerable<IStreamedParameter> group;

            if (identifier.HasValue && streamedParameters.TryGetValue(identifier.Value, out group) &&
                (rawValue != null))
            {
                foreach (var parameter in group)
                {
                    var value = ExtractValue(parameter, rawValue);

                    try
                    {
                        parameter.SetProviderValue(value);
                    }
                    catch (ArgumentException ex)
                    {
                        const string Format = "Read unexpected stream value {0} for the parameter with the path {1}.";
                        throw new ModelException(
                            string.Format(CultureInfo.InvariantCulture, Format, value, parameter.GetPath()), ex);
                    }
                }
            }
        }

        private static object ExtractValue(IStreamedParameter parameter, object rawValue)
        {
            if (parameter.StreamDescriptor.HasValue)
            {
                var rawArray = rawValue as byte[];

                if (rawArray == null)
                {
                    const string Format =
                        "Read stream value {0} while expecting to read an octetstring for the parameter with the path {1}.";
                    throw new ModelException(
                        string.Format(CultureInfo.InvariantCulture, Format, rawValue, parameter.GetPath()));
                }

                return BitConvert(parameter, rawArray);
            }
            else
            {
                return rawValue;
            }
        }

        private static object BitConvert(IStreamedParameter parameter, byte[] rawArray)
        {
            var descriptor = parameter.StreamDescriptor.GetValueOrDefault();
            int offset;
            var array = GetArray(descriptor, rawArray, out offset);

            try
            {
                switch (descriptor.Format)
                {
                    case StreamFormat.Byte:
                        return (long)array[offset];
                    case StreamFormat.UInt16BigEndian:
                    case StreamFormat.UInt16LittleEndian:
                        return (long)BitConverter.ToUInt16(array, offset);
                    case StreamFormat.UInt32BigEndian:
                    case StreamFormat.UInt32LittleEndian:
                        return (long)BitConverter.ToUInt32(array, offset);
                    case StreamFormat.UInt64BigEndian:
                    case StreamFormat.UInt64LittleEndian:
                        return unchecked((long)BitConverter.ToUInt64(array, offset));
                    case StreamFormat.SByte:
                        return (long)unchecked((sbyte)array[offset]);
                    case StreamFormat.Int16BigEndian:
                    case StreamFormat.Int16LittleEndian:
                        return (long)BitConverter.ToInt16(array, offset);
                    case StreamFormat.Int32BigEndian:
                    case StreamFormat.Int32LittleEndian:
                        return (long)BitConverter.ToInt32(array, offset);
                    case StreamFormat.Int64BigEndian:
                    case StreamFormat.Int64LittleEndian:
                        return BitConverter.ToInt64(array, offset);
                    case StreamFormat.Float32BigEndian:
                    case StreamFormat.Float32LittleEndian:
                        return (double)BitConverter.ToSingle(array, offset);
                    default:
                        return BitConverter.ToDouble(array, offset);
                }
            }
            catch (ArgumentException ex)
            {
                throw CreateStreamDescriptorException(parameter, ex);
            }
            catch (IndexOutOfRangeException ex)
            {
                throw CreateStreamDescriptorException(parameter, ex);
            }
        }

        private static byte[] GetArray(StreamDescription descriptor, byte[] rawArray, out int offset)
        {
            offset = Math.Max(0, Math.Min(descriptor.Offset, rawArray.Length));

            if (AreBytesReverse(descriptor.Format))
            {
                var count = Math.Min(1 << (((int)descriptor.Format & 6) >> 1), rawArray.Length - offset);
                var result = new byte[count];
                Array.Copy(rawArray, offset, result, 0, count);
                Array.Reverse(result);
                offset = 0;
                return result;
            }
            else
            {
                return rawArray;
            }
        }

        private static bool AreBytesReverse(StreamFormat format)
        {
            switch (format)
            {
                case StreamFormat.Byte:
                case StreamFormat.SByte:
                    return false;
                default:
                    return (((int)format & 1) == 1) != BitConverter.IsLittleEndian;
            }
        }

        private static ModelException CreateStreamDescriptorException(IStreamedParameter parameter, Exception ex)
        {
            const string Format =
                "A stream entry for the parameter with the path {0} is incompatible, see inner exception for more information.";
            return new ModelException(string.Format(CultureInfo.InvariantCulture, Format, parameter.GetPath()), ex);
        }

        private void RaiseSendRequired() => this.SendRequired?.Invoke(this, EventArgs.Empty);

        private void ReadQualifiedChild(EmberReader reader, ElementType actualType)
        {
            reader.ReadAndAssertOuter(GlowQualifiedNode.Path.OuterId);
            var path = reader.AssertAndReadContentsAsInt32Array();

            if (path.Length == 0)
            {
                throw new ModelException("Invalid path for a qualified element.");
            }

            this.ReadQualifiedChild(reader, actualType, path, 0);
        }
    }
}
