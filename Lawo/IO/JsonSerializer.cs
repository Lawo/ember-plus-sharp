////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.IO
{
    using System.IO;
    using System.Runtime.Serialization.Json;
    using System.Text;

    /// <summary>Serializes objects to the JavaScript Object Notation (JSON) and deserializes JSON data to objects.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public static class JsonSerializer
    {
        /// <summary>Serializes objects to the JavaScript Object Notation (JSON).</summary>
        /// <typeparam name="T">The type to be serialized (data contract).</typeparam>
        /// <param name="data">The data contract object.</param>
        /// <returns>The serialized JSON string.</returns>
        public static string Serialize<T>(T data)
        {
            using (var stream = new MemoryStream())
            {
                new DataContractJsonSerializer(typeof(T)).WriteObject(stream, data);
                stream.Position = 0;

                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>Deserializes JSON data to objects.</summary>
        /// <typeparam name="T">The type to be deserialized (data contract).</typeparam>
        /// <param name="jsonData">The JSON string.</param>
        /// <returns>The data contract object.</returns>
        public static T Deserialize<T>(string jsonData)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonData)))
            {
                return (T)new DataContractJsonSerializer(typeof(T)).ReadObject(stream);
            }
        }
    }
}
