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
    using System.Threading;

    using Ember;

    /// <summary>This API is not intended to be used directly from your code.</summary>
    /// <remarks>Provides the common implementation for all static functions in the object tree
    /// accessible through <see cref="Consumer{T}.Root">Consumer&lt;TRoot&gt;.Root</see>.</remarks>
    /// <typeparam name="TMostDerived">The most-derived subtype of this class.</typeparam>
    /// <threadsafety static="true" instance="false"/>
    public abstract class StaticFunction<TMostDerived> : FunctionBase<TMostDerived>
        where TMostDerived : StaticFunction<TMostDerived>
    {
        internal StaticFunction()
            : base(
                (KeyValuePair<string, ParameterType>[])ArgumentsTemplate.Clone(),
                (KeyValuePair<string, ParameterType>[])ResultTemplate.Clone())
        {
        }

        internal sealed override KeyValuePair<string, ParameterType>[] ReadTupleDescription(
            EmberReader reader, KeyValuePair<string, ParameterType>[] expectedTypes)
        {
            var descriptionCount = this.ReadTupleDescription(reader, expectedTypes, (i, d) => expectedTypes[i] = d);

            if (descriptionCount < expectedTypes.Length)
            {
                throw this.CreateSignatureMismatchException();
            }

            return expectedTypes;
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Method is not public, CA bug?")]
        internal sealed override KeyValuePair<string, ParameterType> ReadTupleItemDescription(
            EmberReader reader, KeyValuePair<string, ParameterType>[] expectedTypes, int index)
        {
            if (index >= expectedTypes.Length)
            {
                throw this.CreateSignatureMismatchException();
            }

            var description = base.ReadTupleItemDescription(reader, expectedTypes, index);

            if (description.Value != expectedTypes[index].Value)
            {
                throw this.CreateSignatureMismatchException();
            }

            return description;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static KeyValuePair<string, ParameterType>[] argumentsTemplate;
        private static KeyValuePair<string, ParameterType>[] resultTemplate;

        private static KeyValuePair<string, ParameterType>[] ArgumentsTemplate =>
            LazyInitializer.EnsureInitialized(ref argumentsTemplate, CreateArgumentsTemplate);

        private static KeyValuePair<string, ParameterType>[] ResultTemplate =>
            LazyInitializer.EnsureInitialized(ref resultTemplate, CreateResultTemplate);

        private static KeyValuePair<string, ParameterType>[] CreateArgumentsTemplate()
        {
            var genericArguments = typeof(TMostDerived).GenericTypeArguments;
            return CreateTypeArray(genericArguments.Where((t, i) => i < genericArguments.Length - 1));
        }

        private static KeyValuePair<string, ParameterType>[] CreateResultTemplate()
        {
            var genericArguments = typeof(TMostDerived).GenericTypeArguments;
            var resultType = genericArguments[genericArguments.Length - 1];
            return CreateTypeArray(
                resultType.IsConstructedGenericType ? resultType.GenericTypeArguments : Enumerable.Empty<Type>());
        }

        private static KeyValuePair<string, ParameterType>[] CreateTypeArray(IEnumerable<Type> argumentTypes) =>
            argumentTypes.Select(t => new KeyValuePair<string, ParameterType>(null, GetType(t))).ToArray();

        private static ParameterType GetType(Type type)
        {
            if (type == typeof(long))
            {
                return ParameterType.Integer;
            }
            else if (type == typeof(double))
            {
                return ParameterType.Real;
            }
            else if (type == typeof(string))
            {
                return ParameterType.String;
            }
            else if (type == typeof(bool))
            {
                return ParameterType.Boolean;
            }
            else if (type == typeof(byte[]))
            {
                return ParameterType.Octets;
            }
            else
            {
                const string Format = "Unsupported type in function signature {0}.";
                throw new ModelException(string.Format(CultureInfo.InvariantCulture, Format, typeof(TMostDerived)));
            }
        }
    }
}
