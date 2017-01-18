////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System.Collections.Generic;

    using Ember;

    internal sealed class DynamicFunction : FunctionBase<DynamicFunction>
    {
        internal DynamicFunction()
            : base(EmptyDescription, EmptyDescription)
        {
        }

        internal sealed override KeyValuePair<string, ParameterType>[] ReadTupleDescription(
            EmberReader reader, KeyValuePair<string, ParameterType>[] expectedTypes)
        {
            var descriptions = new List<KeyValuePair<string, ParameterType>>();
            this.ReadTupleDescription(reader, expectedTypes, (i, d) => descriptions.Add(d));
            return descriptions.ToArray();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static readonly KeyValuePair<string, ParameterType>[] EmptyDescription =
            new KeyValuePair<string, ParameterType>[0];
    }
}
