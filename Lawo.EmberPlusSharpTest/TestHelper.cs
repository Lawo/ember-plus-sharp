using System.Xml;

namespace Lawo.EmberPlusSharpTest
{
    internal class TestHelper
    {
        public static string RandomString(int length = 20)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
        }

        public static bool RandomBoolean()
        {
            return Random.Shared.NextDouble() >= 0.5;
        }

        public static T RandomEnum<T>()
        {
            Type type = typeof(T);
            Array values = type.GetEnumValues();
            int index = Random.Shared.Next(values.Length);

            return (T)values.GetValue(index) ?? default;
        }

        /// <summary>
        /// This is a try to replace the "SoapHexBinary" not found in .NET Core. Using System.Runtime.Remoting.Metadata.W3cXsd2001;
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string? SoapHexBinary(byte[]? value)
        {
            if (value == null)
                return null;
            if (value.Length == 0)
                return "";

            return Convert.ToHexString(value, 0, value.Length);
        }

        /// <summary>
        /// SoapHexBinary.Parse replacement?
        /// </summary>
        /// <returns></returns>
        public static byte[] SoapHexBinaryParse(ReadOnlySpan<char> chars)
        {
            return Convert.FromHexString(chars);
        }

        public static byte[] SoapHexBinaryParse(string chars)
        {
            return Convert.FromHexString(chars);
        }
    }
}
