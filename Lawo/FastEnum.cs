////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>Provides cached implementations for some of the <see cref="Enum"/> methods.</summary>
    /// <threadsafety static="true" instance="false"/>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "We reimplement a part of Enum.")]
    [CLSCompliant(false)]
    public static class FastEnum
    {
        /// <summary>Provides a fast implementation of a part of the functionality of <see cref="Enum.IsDefined"/>.
        /// </summary>
        /// <typeparam name="TEnum">The enumeration type.</typeparam>
        /// <exception cref="ArgumentException"><typeparamref name="TEnum"/> is not an Enum.</exception>
        public static bool IsDefined<TEnum>(TEnum value)
            where TEnum : struct
        {
            return EnumCache<TEnum>.IsDefined(value);
        }

        /// <summary>Provides a fast implementation of a part of the functionality of <see cref="Enum.ToObject"/>.
        /// </summary>
        /// <typeparam name="TEnum">The enumeration type.</typeparam>
        /// <exception cref="ArgumentException"><typeparamref name="TEnum"/> is not an Enum.</exception>
        public static TEnum ToEnum<TEnum>(long value)
            where TEnum : struct
        {
            return EnumCache<TEnum>.ToEnum(value);
        }

        /// <summary>Provides a fast implementation of a part of the functionality of <see cref="Enum.ToObject"/>.
        /// </summary>
        /// <typeparam name="TEnum">The enumeration type.</typeparam>
        /// <exception cref="ArgumentException"><typeparamref name="TEnum"/> is not an Enum.</exception>
        public static TEnum ToEnum<TEnum>(ulong value)
            where TEnum : struct
        {
            return EnumCache<TEnum>.ToEnum(unchecked((long)value));
        }

        /// <summary>Returns the integer represented by <paramref name="value"/>.</summary>
        /// <typeparam name="TEnum">The enumeration type.</typeparam>
        /// <exception cref="ArgumentException"><typeparamref name="TEnum"/> is not an Enum.</exception>
        public static long ToInt64<TEnum>(TEnum value)
            where TEnum : struct
        {
            return EnumCache<TEnum>.ToInt64(value);
        }

        /// <summary>Returns the integer represented by <paramref name="value"/>.</summary>
        /// <typeparam name="TEnum">The enumeration type.</typeparam>
        /// <exception cref="ArgumentException"><typeparamref name="TEnum"/> is not an Enum.</exception>
        public static ulong ToUInt64<TEnum>(TEnum value)
            where TEnum : struct
        {
            return unchecked((ulong)EnumCache<TEnum>.ToInt64(value));
        }

        /// <summary>Returns a dictionary that maps each of the named constants of <typeparamref name="TEnum"/> to its
        /// string representation.</summary>
        /// <typeparam name="TEnum">The enumeration type.</typeparam>
        /// <exception cref="ArgumentException"><typeparamref name="TEnum"/> is not an Enum.</exception>
        public static IReadOnlyDictionary<TEnum, string> GetValueNameMap<TEnum>()
            where TEnum : struct
        {
            return EnumCache<TEnum>.GetValueNameMap();
        }

        /// <summary>Returns a dictionary that maps the string representation of each of the named constants of
        /// <typeparamref name="TEnum"/> to its named constant.</summary>
        /// <typeparam name="TEnum">The enumeration type.</typeparam>
        /// <exception cref="ArgumentException"><typeparamref name="TEnum"/> is not an Enum.</exception>
        public static IReadOnlyDictionary<string, TEnum> GetNameValueMap<TEnum>()
            where TEnum : struct
        {
            return EnumCache<TEnum>.GetNameValueMap();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static class EnumCache<TEnum>
            where TEnum : struct
        {
            internal static bool IsDefined(TEnum value)
            {
                AssertIsEnum();
                return ValueNameMap.ContainsKey(value);
            }

            internal static TEnum ToEnum(long value)
            {
                AssertIsEnum();
                return ConvertToEnum(value);
            }

            internal static long ToInt64(TEnum value)
            {
                AssertIsEnum();
                return ConvertToInt64(value);
            }

            internal static IReadOnlyDictionary<TEnum, string> GetValueNameMap()
            {
                AssertIsEnum();
                return ValueNameMap;
            }

            internal static IReadOnlyDictionary<string, TEnum> GetNameValueMap()
            {
                AssertIsEnum();
                return NameValueMap;
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////

            private static readonly Dictionary<TEnum, string> ValueNameMap;
            private static readonly Dictionary<string, TEnum> NameValueMap;
            private static readonly Func<long, TEnum> ConvertToEnum;
            private static readonly Func<TEnum, long> ConvertToInt64;

            [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Inline initialization would be less efficient.")]
            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Complexity cannot be reduced in a meaningful way.")]
#pragma warning disable SA1202 // Elements must be ordered by access. Incorrectly identified as public, TODO: File bug.
            static EnumCache()
            {
                var values = GetValues();
                var underlyingType = GetUnderlyingType();

                if ((values == null) || (underlyingType == null))
                {
                    return;
                }

                ValueNameMap = new Dictionary<TEnum, string>();
                NameValueMap = new Dictionary<string, TEnum>();

                foreach (var value in values)
                {
                    var str = value.ToString();
                    ValueNameMap.Add(value, str);
                    NameValueMap.Add(str, value);
                }

                switch (underlyingType.Name)
                {
                    case "SByte":
                        ConvertToEnum = v => (TEnum)(object)unchecked((sbyte)v);
                        ConvertToInt64 = v => (sbyte)(object)v;
                        break;
                    case "Int16":
                        ConvertToEnum = v => (TEnum)(object)unchecked((short)v);
                        ConvertToInt64 = v => (short)(object)v;
                        break;
                    case "Int32":
                        ConvertToEnum = v => (TEnum)(object)unchecked((int)v);
                        ConvertToInt64 = v => (int)(object)v;
                        break;
                    case "Byte":
                        ConvertToEnum = v => (TEnum)(object)unchecked((byte)v);
                        ConvertToInt64 = v => (byte)(object)v;
                        break;
                    case "UInt16":
                        ConvertToEnum = v => (TEnum)(object)unchecked((ushort)v);
                        ConvertToInt64 = v => (ushort)(object)v;
                        break;
                    case "UInt32":
                        ConvertToEnum = v => (TEnum)(object)unchecked((uint)v);
                        ConvertToInt64 = v => (uint)(object)v;
                        break;
                    case "UInt64":
                        ConvertToEnum = v => (TEnum)(object)unchecked((ulong)v);
                        ConvertToInt64 = v => unchecked((long)(ulong)(object)v);
                        break;
                    default:
                        ConvertToEnum = v => (TEnum)(object)v;
                        ConvertToInt64 = v => (long)(object)v;
                        break;
                }
            }
#pragma warning restore SA1202 // Elements must be ordered by access

            private static TEnum[] GetValues()
            {
                try
                {
                    return (TEnum[])Enum.GetValues(typeof(TEnum));
                }
                catch (ArgumentException)
                {
                    return null;
                }
            }

            private static Type GetUnderlyingType()
            {
                try
                {
                    return Enum.GetUnderlyingType(typeof(TEnum));
                }
                catch (ArgumentException)
                {
                    return null;
                }
            }

            [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "We throw for a generic parameter.")]
            private static void AssertIsEnum()
            {
                if (ValueNameMap == null)
                {
                    throw new ArgumentException("The provided type must be an Enum", "TEnum");
                }
            }
        }
    }
}
