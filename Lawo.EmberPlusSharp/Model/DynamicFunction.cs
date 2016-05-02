////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2016 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    using Lawo.EmberPlusSharp.Ember;

    internal sealed class DynamicFunction : FunctionBase<DynamicFunction>
    {
        private static readonly KeyValuePair<string, ParameterType>[] EmptyDescription =
            new KeyValuePair<string, ParameterType>[0];

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal DynamicFunction()
            : base(EmptyDescription, EmptyDescription)
        {
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Method is not public, CA bug?")]
        internal sealed override KeyValuePair<string, ParameterType>[] ReadTupleDescription(
            EmberReader reader, KeyValuePair<string, ParameterType>[] expectedTypes)
        {
            var descriptions = new List<KeyValuePair<string, ParameterType>>();
            this.ReadTupleDescription(reader, expectedTypes, (i, d) => descriptions.Add(d));
            return descriptions.ToArray();
        }
    }
}
