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

    using Ember;
    using Glow;

    /// <summary>This class is not intended to be referenced in your code.</summary>
    /// <remarks>Provides the common implementation for all parameters in the object tree accessible through
    /// <see cref="Consumer{T}.Root">Consumer&lt;TRoot&gt;.Root</see>.</remarks>
    /// <typeparam name="TMostDerived">The most-derived subtype of this class.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <threadsafety static="true" instance="false"/>
    public abstract class ParameterBase<TMostDerived, TValue> : ElementWithSchemas<TMostDerived>, IStreamedParameter
        where TMostDerived : ParameterBase<TMostDerived, TValue>
    {
        /// <inheritdoc/>
        public ParameterAccess Access
        {
            get { return this.access; }
            private set { this.SetValue(ref this.access, value); }
        }

        /// <inheritdoc/>
        public string Format
        {
            get { return this.format; }
            private set { this.SetValue(ref this.format, value); }
        }

        /// <inheritdoc cref="IParameter.DefaultValue"/>
        public TValue DefaultValue
        {
            get { return this.defaultValue; }
            private set { this.SetValue(ref this.defaultValue, value); }
        }

        /// <inheritdoc/>
        public ParameterType Type
        {
            get { return this.type.Value; }
            private set { this.SetValue(ref this.type, value); }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Required property is provided by subclasses.")]
        object IParameter.Value
        {
            get { return this.ValueCore; }
            set { this.ValueCore = this.AssertValueType(value); }
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "NumericParameter provides strongly-typed property, not relevant for all other parameters.")]
        object IParameter.Minimum => this.GetMinimum();

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "NumericParameter provides strongly-typed property, not relevant for all other parameters.")]
        object IParameter.Maximum => this.GetMaximum();

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "IntegerParameter provides strongly-typed property, not relevant for all other parameters.")]
        int? IParameter.Factor => this.FactorCore;

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "NumericParameter provides strongly-typed property, not relevant for all other parameters.")]
        string IParameter.Formula => this.FormulaCore;

        /// <inheritdoc/>
        object IParameter.DefaultValue => this.DefaultValue;

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "IntegerParameter and EnumParameter provide strongly-typed property, not relevant for all other parameters.")]
        IReadOnlyList<KeyValuePair<string, int>> IParameter.EnumMap => this.EnumMapCore;

        /// <inheritdoc/>
        int? IStreamedParameter.StreamIdentifier => this.StreamIdentifier;

        /// <inheritdoc/>
        StreamDescription? IStreamedParameter.StreamDescriptor => this.StreamDescriptor;

        /// <inheritdoc/>
        void IStreamedParameter.SetProviderValue(object value) => this.SetProviderValue(this.AssertValueType(value));

        internal ParameterBase()
            : base(RetrievalState.Complete)
        {
        }

        internal TValue ValueCore
        {
            get
            {
                if ((this.access & ParameterAccess.Read) == 0)
                {
                    const string Format = "The parameter with the path {0} does not allow read access.";
                    throw new InvalidOperationException(
                        string.Format(CultureInfo.InvariantCulture, Format, this.GetPath()));
                }

                return this.theValue;
            }

            set
            {
                if ((this.access & ParameterAccess.Write) == 0)
                {
                    const string Format = "The parameter with the path {0} does not allow write access.";
                    throw new InvalidOperationException(
                        string.Format(CultureInfo.InvariantCulture, Format, this.GetPath()));
                }

                if (this.type == ParameterType.Trigger)
                {
                    this.HasChanges = true;
                }
                else
                {
                    if (value == null)
                    {
                        throw new ArgumentNullException(nameof(value));
                    }

                    this.SetConsumerValue(ref this.theValue, value, "Value");
                }
            }
        }

        internal virtual int? FactorCore
        {
            get { return null; }
            set { value.Ignore(); }
        }

        internal virtual string FormulaCore
        {
            get { return null; }
            set { value.Ignore(); }
        }

        internal virtual IReadOnlyList<KeyValuePair<string, int>> EnumMapCore
        {
            get { return null; }
            set { value.Ignore(); }
        }

        internal TValue GetValue() => this.theValue;

        internal abstract TValue ReadValue(EmberReader reader, out ParameterType? parameterType);

        internal abstract void WriteValue(EmberWriter writer, TValue value);

        internal virtual object GetMinimum() => null;

        internal virtual void SetMinimum(TValue value)
        {
        }

        internal virtual object GetMaximum() => null;

        internal virtual void SetMaximum(TValue value)
        {
        }

        internal virtual TValue AssertValueType(object value)
        {
            try
            {
                return (TValue)value;
            }
            catch (InvalidCastException ex)
            {
                throw new ArgumentException(
                    "The type of value does not match the type of the parameter.", nameof(value), ex);
            }
        }

        internal sealed override void ResetRetrievalState()
        {
            if (this.StreamIdentifier.HasValue)
            {
                base.ResetRetrievalState();
            }
        }

        internal sealed override bool WriteRequest(EmberWriter writer, IStreamedParameterCollection streamedParameters)
        {
            if (this.RetrievalState.Equals(RetrievalState.None))
            {
                writer.WriteStartApplicationDefinedType(
                    GlowElementCollection.Element.OuterId, GlowQualifiedParameter.InnerNumber);
                writer.WriteValue(GlowQualifiedParameter.Path.OuterId, this.NumberPath);
                writer.WriteStartApplicationDefinedType(
                    GlowQualifiedParameter.Children.OuterId, GlowElementCollection.InnerNumber);
                this.WriteCommandCollection(writer, GlowCommandNumber.Subscribe, RetrievalState.Complete);
                writer.WriteEndContainer();
                writer.WriteEndContainer();
                streamedParameters.Add(this);
            }

            return false;
        }

        internal override RetrievalState ReadContents(EmberReader reader, ElementType actualType)
        {
            this.AssertElementType(ElementType.Parameter, actualType);

            ParameterType? valueType = null;
            ParameterType? enumType = null;
            ParameterType? typeType = null;

            while (reader.Read() && (reader.InnerNumber != InnerNumber.EndContainer))
            {
                ParameterType? dummyType;

                switch (reader.GetContextSpecificOuterNumber())
                {
                    case GlowParameterContents.Description.OuterNumber:
                        this.Description = reader.AssertAndReadContentsAsString();
                        break;
                    case GlowParameterContents.Value.OuterNumber:
                        this.SetProviderValue(this.ReadValue(reader, out valueType));
                        break;
                    case GlowParameterContents.Minimum.OuterNumber:
                        this.SetMinimum(this.ReadValue(reader, out dummyType));
                        break;
                    case GlowParameterContents.Maximum.OuterNumber:
                        this.SetMaximum(this.ReadValue(reader, out dummyType));
                        break;
                    case GlowParameterContents.Access.OuterNumber:
                        this.Access = this.ReadEnum<ParameterAccess>(reader, GlowParameterContents.Access.Name);
                        break;
                    case GlowParameterContents.Format.OuterNumber:
                        this.Format = reader.AssertAndReadContentsAsString();
                        break;
                    case GlowParameterContents.Enumeration.OuterNumber:
                        this.EnumMapCore = ReadEnumeration(reader);
                        enumType = ParameterType.Enum;
                        break;
                    case GlowParameterContents.Factor.OuterNumber:
                        this.FactorCore = this.ReadInt(reader, GlowParameterContents.Factor.Name);
                        break;
                    case GlowParameterContents.IsOnline.OuterNumber:
                        this.IsOnline = reader.AssertAndReadContentsAsBoolean();
                        var send = (this.RetrieveDetailsChangeStatus == RetrieveDetailsChangeStatus.Changed) &&
                            this.RetrieveDetails && this.StreamIdentifier.HasValue;
                        this.RetrievalState &= send ? RetrievalState.None : RetrievalState.Complete;
                        break;
                    case GlowParameterContents.Formula.OuterNumber:
                        this.FormulaCore = reader.AssertAndReadContentsAsString();
                        break;
                    case GlowParameterContents.Step.OuterNumber:
                        this.ReadInt(reader, GlowParameterContents.Step.Name);
                        break;
                    case GlowParameterContents.Default.OuterNumber:
                        this.DefaultValue = this.ReadValue(reader, out dummyType);
                        break;
                    case GlowParameterContents.Type.OuterNumber:
                        typeType = this.ReadEnum<ParameterType>(reader, GlowParameterContents.Type.Name);
                        break;
                    case GlowParameterContents.StreamIdentifier.OuterNumber:
                        this.StreamIdentifier = this.ReadInt(reader, GlowParameterContents.StreamIdentifier.Name);
                        break;
                    case GlowParameterContents.EnumMap.OuterNumber:
                        this.EnumMapCore = this.ReadEnumMap(reader);
                        enumType = ParameterType.Enum;
                        break;
                    case GlowParameterContents.StreamDescriptor.OuterNumber:
                        this.StreamDescriptor = this.ReadStreamDescriptor(reader);
                        break;
                    case GlowParameterContents.SchemaIdentifiers.OuterNumber:
                        this.ReadSchemaIdentifiers(reader);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            this.SetFinalTytpe(valueType, enumType, typeType);
            return this.RetrievalState;
        }

        internal sealed override void WriteChanges(EmberWriter writer, IInvocationCollection pendingInvocations)
        {
            if (this.HasChanges)
            {
                writer.WriteStartApplicationDefinedType(
                    GlowElementCollection.Element.OuterId, GlowQualifiedParameter.InnerNumber);

                writer.WriteValue(GlowQualifiedParameter.Path.OuterId, this.NumberPath);
                writer.WriteStartSet(GlowQualifiedParameter.Contents.OuterId);

                if (this.theValue == null)
                {
                    // This can only happen when the parameter happens to be a trigger.
                    writer.WriteValue(GlowParameterContents.Value.OuterId, 0);
                }
                else
                {
                    this.WriteValue(writer, this.theValue);
                }

                writer.WriteEndContainer();
                writer.WriteEndContainer();
                this.HasChanges = false;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static List<KeyValuePair<string, int>> ReadEnumeration(EmberReader reader)
        {
            var entries = reader.AssertAndReadContentsAsString().Split('\n');
            var result = new List<KeyValuePair<string, int>>();

            for (int index = 0; index < entries.Length; ++index)
            {
                var entryString = entries[index];

                if (!entryString.StartsWith("~", StringComparison.Ordinal))
                {
                    result.Add(new KeyValuePair<string, int>(entryString, index));
                }
            }

            return result;
        }

        private TValue theValue;
        private ParameterAccess access = ParameterAccess.Read;
        private string format;
        private TValue defaultValue;
        private ParameterType? type;
        private int? streamIdentifier;

        private int? StreamIdentifier
        {
            get
            {
                return this.streamIdentifier;
            }

            set
            {
                if (this.streamIdentifier != value)
                {
                    this.streamIdentifier = value;

                    if (value.HasValue && this.RetrieveDetails)
                    {
                        this.RetrievalState = RetrievalState.None;
                    }
                }
            }
        }

        private StreamDescription? StreamDescriptor { get; set; }

        private void SetProviderValue(TValue value)
        {
            if (!this.HasChanges)
            {
                this.SetValue(ref this.theValue, value, "Value");
            }
        }

        private List<KeyValuePair<string, int>> ReadEnumMap(EmberReader reader)
        {
            reader.AssertInnerNumber(GlowStringIntegerCollection.InnerNumber);
            var result = new List<KeyValuePair<string, int>>();

            while (reader.Read() && (reader.InnerNumber != InnerNumber.EndContainer))
            {
                reader.AssertInnerNumber(GlowStringIntegerPair.InnerNumber);
                reader.ReadAndAssertOuter(GlowStringIntegerPair.EntryString.OuterId);
                var entryString = reader.AssertAndReadContentsAsString();
                reader.ReadAndAssertOuter(GlowStringIntegerPair.EntryInteger.OuterId);
                var entryInteger = this.ReadInt(reader, GlowStringIntegerPair.EntryInteger.Name);

                while (reader.Read() && (reader.InnerNumber != InnerNumber.EndContainer))
                {
                    reader.Skip();
                }

                result.Add(new KeyValuePair<string, int>(entryString, entryInteger));
            }

            return result;
        }

        private StreamDescription ReadStreamDescriptor(EmberReader reader)
        {
            reader.AssertInnerNumber(GlowStreamDescription.InnerNumber);
            reader.ReadAndAssertOuter(GlowStreamDescription.Format.OuterId);
            var streamFormat = this.ReadEnum<StreamFormat>(reader, GlowStreamDescription.Format.Name);
            reader.ReadAndAssertOuter(GlowStreamDescription.Offset.OuterId);
            var offset = this.ReadInt(reader, GlowStreamDescription.Offset.Name);

            while (reader.Read() && (reader.InnerNumber != InnerNumber.EndContainer))
            {
                reader.Skip();
            }

            return new StreamDescription(streamFormat, offset);
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "enumMap", Justification = "Official Glow name.")]
        private void SetFinalTytpe(ParameterType? valueType, ParameterType? enumType, ParameterType? typeType)
        {
            var finalType =
                this.type ?? (typeType == ParameterType.Trigger ? typeType : null) ?? enumType ?? valueType ?? typeType;

            if (finalType.HasValue)
            {
                this.Type = finalType.Value;
            }
            else
            {
                if (this.RetrieveDetails)
                {
                    const string Format =
                        "No enumeration, enumMap, value or type field is available for the parameter with the path {0}.";
                    throw new ModelException(string.Format(CultureInfo.InvariantCulture, Format, this.GetPath()));
                }
            }
        }
    }
}
