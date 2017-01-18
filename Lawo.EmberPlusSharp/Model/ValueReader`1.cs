////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System;

    using Ember;

    internal sealed class ValueReader<T> : IValueReader
    {
        object IValueReader.Value => this.Value;

        void IValueReader.ReadValue(EmberReader reader) => this.Value = ReadValueCore(reader);

        internal T Value { get; private set; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static readonly Func<EmberReader, T> ReadValueCore = (Func<EmberReader, T>)GetReadValueCore();

        private static Delegate GetReadValueCore()
        {
            var valueType = typeof(T);

            if (valueType == typeof(long))
            {
                return (Func<EmberReader, long>)EmberReaderExtensions.AssertAndReadContentsAsInt64;
            }
            else if (valueType == typeof(double))
            {
                return (Func<EmberReader, double>)EmberReaderExtensions.AssertAndReadContentsAsDouble;
            }
            else if (valueType == typeof(string))
            {
                return (Func<EmberReader, string>)EmberReaderExtensions.AssertAndReadContentsAsString;
            }
            else if (valueType == typeof(bool))
            {
                return (Func<EmberReader, bool>)EmberReaderExtensions.AssertAndReadContentsAsBoolean;
            }
            else
            {
                return (Func<EmberReader, byte[]>)EmberReaderExtensions.AssertAndReadContentsAsByteArray;
            }
        }
    }
}
