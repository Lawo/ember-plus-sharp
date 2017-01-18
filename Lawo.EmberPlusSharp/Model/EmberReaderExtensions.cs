////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System;
    using System.Globalization;

    using Ember;

    internal static class EmberReaderExtensions
    {
        internal static bool AssertAndReadContentsAsBoolean(this EmberReader reader) =>
            AssertAndReadContents(reader, r => r.ReadContentsAsBoolean());

        internal static int AssertAndReadContentsAsInt32(this EmberReader reader)
        {
            var result = AssertAndReadContentsAsInt64(reader);

            if ((result < int.MinValue) || (result > int.MaxValue))
            {
                const string Format = "Found actual integer {0} while expecting to read a 32-bit integer.";
                throw new ModelException(string.Format(CultureInfo.InvariantCulture, Format, result));
            }

            return (int)result;
        }

        internal static long AssertAndReadContentsAsInt64(this EmberReader reader) =>
            AssertAndReadContents(reader, r => r.ReadContentsAsInt64());

        internal static byte[] AssertAndReadContentsAsByteArray(this EmberReader reader) =>
            AssertAndReadContents(reader, r => r.ReadContentsAsByteArray());

        internal static double AssertAndReadContentsAsDouble(this EmberReader reader) =>
            AssertAndReadContents(reader, r => r.ReadContentsAsDouble());

        internal static string AssertAndReadContentsAsString(this EmberReader reader) =>
            AssertAndReadContents(reader, r => r.ReadContentsAsString());

        internal static int[] AssertAndReadContentsAsInt32Array(this EmberReader reader) =>
            AssertAndReadContents(reader, r => r.ReadContentsAsInt32Array());

        internal static void ReadAndAssertOuter(this EmberReader reader, EmberId expectedOuter)
        {
            AssertRead(reader, expectedOuter);

            if (reader.InnerNumber == InnerNumber.EndContainer)
            {
                const string Format = "Found end of container while expecting outer identifier {0}.";
                throw new ModelException(string.Format(CultureInfo.InvariantCulture, Format, expectedOuter));
            }

            if (reader.OuterId != expectedOuter)
            {
                const string Format = "Found actual outer identifier {0} while expecting {1}.";
                throw new ModelException(
                    string.Format(CultureInfo.InvariantCulture, Format, reader.OuterId, expectedOuter));
            }
        }

        internal static int GetContextSpecificOuterNumber(this EmberReader reader)
        {
            var outerId = reader.OuterId;

            if (outerId.Class != Class.ContextSpecific)
            {
                const string Format = "Found actual outer identifier {0} while expecting a context-specific one.";
                throw new ModelException(string.Format(CultureInfo.InvariantCulture, Format, outerId));
            }

            return outerId.Number;
        }

        internal static void AssertInnerNumber(this EmberReader reader, int expectedInnerNumber)
        {
            if (reader.InnerNumber != expectedInnerNumber)
            {
                const string Format =
                    "Found actual inner number {0} while expecting {1} on a container with the outer identifier {2}.";
                throw new ModelException(string.Format(
                    CultureInfo.InvariantCulture, Format, reader.InnerNumber, expectedInnerNumber, reader.OuterId));
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static void AssertRead(EmberReader reader, EmberId? expectedOuter)
        {
            try
            {
                if (!reader.Read())
                {
                    const string Format = "Encountered end of stream while expecting outer identifier{0}.";
                    throw new ModelException(string.Format(CultureInfo.InvariantCulture, Format, GetId(expectedOuter)));
                }
            }
            catch (EmberException ex)
            {
                const string Format = "Encountered invalid EmBER data while expecting outer identifier{0}.";
                throw new ModelException(string.Format(CultureInfo.InvariantCulture, Format, GetId(expectedOuter)), ex);
            }
        }

        private static T AssertAndReadContents<T>(EmberReader reader, Func<EmberReader, T> read)
        {
            try
            {
                return read(reader);
            }
            catch (InvalidOperationException ex)
            {
                const string Format =
                    "Found actual inner number {0} while expecting to read a {1} data value with outer identifier {2}.";
                throw new ModelException(
                    string.Format(CultureInfo.InvariantCulture, Format, reader.InnerNumber, typeof(T), reader.OuterId),
                    ex);
            }
        }

        private static string GetId(EmberId? expectedOuter) =>
            string.Format(CultureInfo.InvariantCulture, "{0}{1}", expectedOuter == null ? null : " ", expectedOuter);
    }
}
