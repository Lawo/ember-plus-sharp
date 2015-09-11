﻿////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model
{
    using System;

    using Ember;

    internal sealed class ValueReader<T> : IValueReader
    {
        private static readonly Func<EmberReader, T> ReadValueCore = GetReadValueCore();

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        object IValueReader.Value
        {
            get { return this.Value; }
        }

        void IValueReader.ReadValue(EmberReader reader)
        {
            this.Value = ReadValueCore(reader);
        }

        internal T Value { get; private set; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static Func<EmberReader, T> GetReadValueCore()
        {
            var valueType = typeof(T);

            if (valueType == typeof(long))
            {
                return (Func<EmberReader, T>)(Delegate)(Func<EmberReader, long>)
                    EmberReaderExtensions.AssertAndReadContentsAsInt64;
            }
            else if (valueType == typeof(double))
            {
                return (Func<EmberReader, T>)(Delegate)(Func<EmberReader, double>)
                    EmberReaderExtensions.AssertAndReadContentsAsDouble;
            }
            else if (valueType == typeof(string))
            {
                return (Func<EmberReader, T>)(Delegate)(Func<EmberReader, string>)
                    EmberReaderExtensions.AssertAndReadContentsAsString;
            }
            else if (valueType == typeof(bool))
            {
                return (Func<EmberReader, T>)(Delegate)(Func<EmberReader, bool>)
                    EmberReaderExtensions.AssertAndReadContentsAsBoolean;
            }
            else
            {
                return (Func<EmberReader, T>)(Delegate)(Func<EmberReader, byte[]>)
                    EmberReaderExtensions.AssertAndReadContentsAsByteArray;
            }
        }
    }
}
