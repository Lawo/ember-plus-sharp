////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System.Collections.Generic;
    using System.Linq;

    internal sealed class DynamicResult : ResultBase<DynamicResult>
    {
        internal DynamicResult(IReadOnlyList<KeyValuePair<string, ParameterType>> expectedTypes)
            : base(expectedTypes.Select(CreateReader).ToArray())
        {
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static IValueReader CreateReader(KeyValuePair<string, ParameterType> expectedType)
        {
            switch (expectedType.Value)
            {
                case ParameterType.Integer:
                    return new ValueReader<long>();
                case ParameterType.Real:
                    return new ValueReader<double>();
                case ParameterType.String:
                    return new ValueReader<string>();
                case ParameterType.Boolean:
                    return new ValueReader<bool>();
                default:
                    return new ValueReader<byte[]>();
            }
        }
    }
}
