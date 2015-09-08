////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.IO
{
    using System.IO;
    using System.Runtime.Serialization.Json;
    using System.Text;

    /// <summary>
    /// Serializes objects to the JavaScript Object Notation (JSON) and deserializes JSON data to objects.
    /// </summary>
    public static class JsonSerializer
    {
        /// <summary>
        /// Serializes objects to the JavaScript Object Notation (JSON).
        /// </summary>
        /// <typeparam name="T">The type to be serialized (data contract).</typeparam>
        /// <param name="data">The data contract object.</param>
        /// <returns>The serialized JSON string.</returns>
        public static string Serialize<T>(T data)
        {
            var stream = new MemoryStream();

            try
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                serializer.WriteObject(stream, data);
                string jsonData = Encoding.UTF8.GetString(stream.ToArray(), 0, (int)stream.Length);
                stream.Dispose();

                return jsonData;
            }
            catch
            {
                stream.Dispose();

                throw;
            }
        }

        /// <summary>
        /// Deserializes JSON data to objects.
        /// </summary>
        /// <typeparam name="T">The type to be deserialized (data contract).</typeparam>
        /// <param name="jsonData">The JSON string.</param>
        /// <returns>The data contract object.</returns>
        public static T Deserialize<T>(string jsonData)
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonData));

            try
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                T data = (T)serializer.ReadObject(stream);
                stream.Dispose();

                return data;
            }
            catch
            {
                stream.Dispose();

                throw;
            }
        }
    }
}
