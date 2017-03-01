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
    using System.Linq;
    using System.Threading.Tasks;

    using Ember;
    using Glow;

    /// <summary>This API is not intended to be used directly from your code.</summary>
    /// <remarks>Provides the common implementation for all functions.</remarks>
    /// <typeparam name="TMostDerived">The most-derived subtype of this class.</typeparam>
    /// <threadsafety static="true" instance="false"/>
    public abstract class FunctionBase<TMostDerived> : Element<TMostDerived>, IFunction
        where TMostDerived : FunctionBase<TMostDerived>
    {
        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Information is only relevant for interface client code.")]
        IReadOnlyList<KeyValuePair<string, ParameterType>> IFunction.Arguments => this.arguments;

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Information is only relevant for interface client code.")]
        IReadOnlyList<KeyValuePair<string, ParameterType>> IFunction.Result => this.result;

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Only relevant for interface client code.")]
        Task<IResult> IFunction.InvokeAsync(params object[] actualArguments) => this.InvokeCoreAsync(actualArguments);

        internal FunctionBase(
            KeyValuePair<string, ParameterType>[] arguments, KeyValuePair<string, ParameterType>[] result)
        {
            this.arguments = arguments;
            this.result = result;
        }

        internal Task<TResult> InvokeCoreAsync<TResult>(TResult invokeResult, params Action<EmberWriter>[] writers)
            where TResult : ResultBase<TResult>
        {
            this.invocations.Enqueue(new KeyValuePair<IInvocationResult, Action<EmberWriter>[]>(invokeResult, writers));
            this.HasChanges = true;
            return invokeResult.Task;
        }

        internal override RetrievalState ReadContents(EmberReader reader, ElementType actualType)
        {
            this.AssertElementType(ElementType.Function, actualType);
            var isEmpty = true;
            var argumentsRead = false;
            var resultRead = false;

            while (reader.Read() && (reader.InnerNumber != InnerNumber.EndContainer))
            {
                isEmpty = false;

                switch (reader.GetContextSpecificOuterNumber())
                {
                    case GlowFunctionContents.Description.OuterNumber:
                        this.Description = reader.AssertAndReadContentsAsString();
                        break;
                    case GlowFunctionContents.Arguments.OuterNumber:
                        this.arguments = this.ReadTupleDescription(reader, this.arguments);
                        argumentsRead = true;
                        break;
                    case GlowFunctionContents.Result.OuterNumber:
                        this.result = this.ReadTupleDescription(reader, this.result);
                        resultRead = true;
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            if (!isEmpty &&
                ((!argumentsRead && (this.arguments.Length > 0)) || (!resultRead && (this.result.Length > 0))))
            {
                throw this.CreateSignatureMismatchException();
            }

            return RetrievalState.Complete;
        }

        internal sealed override RetrievalState ReadAdditionalField(EmberReader reader, int contextSpecificOuterNumber)
        {
            reader.Skip();
            return RetrievalState.Complete;
        }

        internal sealed override void WriteChanges(EmberWriter writer, IInvocationCollection pendingInvocations)
        {
            if (this.HasChanges)
            {
                writer.WriteStartApplicationDefinedType(
                    GlowElementCollection.Element.OuterId, GlowQualifiedFunction.InnerNumber);

                writer.WriteValue(GlowQualifiedFunction.Path.OuterId, this.NumberPath);
                writer.WriteStartApplicationDefinedType(
                    GlowQualifiedFunction.Children.OuterId, GlowElementCollection.InnerNumber);
                this.WriteInvocations(writer, pendingInvocations);
                writer.WriteEndContainer();
                writer.WriteEndContainer();
                this.HasChanges = false;
            }
        }

        internal abstract KeyValuePair<string, ParameterType>[] ReadTupleDescription(
            EmberReader reader, KeyValuePair<string, ParameterType>[] expectedTypes);

        internal virtual KeyValuePair<string, ParameterType> ReadTupleItemDescription(
            EmberReader reader, KeyValuePair<string, ParameterType>[] expectedTypes, int index)
        {
            reader.AssertInnerNumber(GlowTupleItemDescription.InnerNumber);
            reader.ReadAndAssertOuter(GlowTupleItemDescription.Type.OuterId);
            var type = this.ReadEnum<ParameterType>(reader, GlowTupleItemDescription.Type.Name);
            string name = null;

            while (reader.Read() && (reader.InnerNumber != InnerNumber.EndContainer))
            {
                switch (reader.GetContextSpecificOuterNumber())
                {
                    case GlowTupleItemDescription.TheName.OuterNumber:
                        name = reader.AssertAndReadContentsAsString();
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            return new KeyValuePair<string, ParameterType>(name, type);
        }

        internal int ReadTupleDescription(
            EmberReader reader,
            KeyValuePair<string, ParameterType>[] expectedTypes,
            Action<int, KeyValuePair<string, ParameterType>> addItem)
        {
            reader.AssertInnerNumber(GlowTupleDescription.InnerNumber);
            int index;

            for (index = 0; reader.Read() && (reader.InnerNumber != InnerNumber.EndContainer);)
            {
                if (reader.GetContextSpecificOuterNumber() == GlowTupleDescription.TupleItemDescription.OuterNumber)
                {
                    addItem(index, this.ReadTupleItemDescription(reader, expectedTypes, index));
                    ++index;
                }
                else
                {
                    reader.Skip();
                }
            }

            return index;
        }

        internal ModelException CreateSignatureMismatchException()
        {
            const string Format =
                "The actual signature for the function with the path {0} does not match the expected signature.";
            return new ModelException(string.Format(CultureInfo.InvariantCulture, Format, this.GetPath()));
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static Action<EmberWriter> CreateWriter(
            KeyValuePair<string, ParameterType> expectedType, object argument)
        {
            try
            {
                switch (expectedType.Value)
                {
                    case ParameterType.Integer:
                        return new ValueWriter<long>((long)argument).WriteValue;
                    case ParameterType.Real:
                        return new ValueWriter<double>((double)argument).WriteValue;
                    case ParameterType.String:
                        return new ValueWriter<string>((string)argument).WriteValue;
                    case ParameterType.Boolean:
                        return new ValueWriter<bool>((bool)argument).WriteValue;
                    default:
                        return new ValueWriter<byte[]>((byte[])argument).WriteValue;
                }
            }
            catch (InvalidCastException ex)
            {
                throw new ArgumentException(
                    "The type of at least one actual argument is not equal to the expected type.", ex);
            }
        }

        private readonly Queue<KeyValuePair<IInvocationResult, Action<EmberWriter>[]>> invocations =
            new Queue<KeyValuePair<IInvocationResult, Action<EmberWriter>[]>>();

        private KeyValuePair<string, ParameterType>[] arguments;
        private KeyValuePair<string, ParameterType>[] result;

        private async Task<IResult> InvokeCoreAsync(object[] actualArguments)
        {
            if (actualArguments.Length != this.arguments.Length)
            {
                throw new ArgumentException(
                    "The number of actual arguments is not equal to the number of expected arguments.");
            }

            return await this.InvokeCoreAsync(
                new DynamicResult(this.result), this.arguments.Zip(actualArguments, CreateWriter).ToArray());
        }

        private void WriteInvocations(EmberWriter writer, IInvocationCollection pendingInvocations)
        {
            while (this.invocations.Count > 0)
            {
                writer.WriteStartApplicationDefinedType(GlowElementCollection.Element.OuterId, GlowCommand.InnerNumber);
                writer.WriteValue(GlowCommand.Number.OuterId, 33);
                writer.WriteStartApplicationDefinedType(GlowCommand.Invocation.OuterId, GlowInvocation.InnerNumber);
                var invocation = this.invocations.Dequeue();
                var invocationId = pendingInvocations.Add(invocation.Key);
                writer.WriteValue(GlowInvocation.InvocationId.OuterId, invocationId);
                writer.WriteStartSequence(GlowInvocation.Arguments.OuterId);

                foreach (var writeValue in invocation.Value)
                {
                    writeValue(writer);
                }

                writer.WriteEndContainer();
                writer.WriteEndContainer();
                writer.WriteEndContainer();
            }
        }
    }
}
