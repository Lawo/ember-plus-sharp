////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.IO
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using UnitTesting;

    /// <summary>Tests <see cref="JsonSerializer"/>.</summary>
    [TestClass]
    public class JsonSerializerTest : TestBase
    {
        /// <summary>Test the main use case. Serialize an object to string and deserialize it again.</summary>
        [TestMethod]
        public void SerializeAndDeserialize()
        {
            var original = new TestDataContract { Text = "Hello", Number = 4 };

            var message = JsonSerializer.Serialize(original);
            var result = JsonSerializer.Deserialize<TestDataContract>(message);

            Assert.AreEqual(original.Number, result.Number);
            Assert.AreEqual(original.Text, result.Text);
            Assert.AreEqual(message, JsonSerializer.Serialize(result));
        }

        /// <summary>Try to serialize an non serializable object.</summary>
        [TestMethod]
        public void NoSerialization()
        {
            var original = new TestNoDataContract { Text = "Hello." };

            AssertThrow<InvalidDataContractException>(() => JsonSerializer.Serialize(original));
        }

        /// <summary>Try to deserialize a non de-serializable object.</summary>
        [TestMethod]
        public void NoDeserialization()
        {
            string message = "{\"Number\":4,\"Text\":\"Hello\"}";

            AssertThrow<InvalidDataContractException>(() => JsonSerializer.Deserialize<TestNoDataContract>(message));
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [DataContract]
        private class TestDataContract
        {
            [DataMember]
            public string Text
            {
                get;
                set;
            }

            [DataMember]
            public int Number
            {
                get;
                set;
            }
        }

        private class TestNoDataContract
        {
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Used by Data Contract Serialializer")]
            public string Text
            {
                get;
                set;
            }
        }
    }
}
