////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Ember
{
    using System;
    using System.Xml;

    /// <summary>Provides a method to convert EmBER-encoded data to XML.</summary>
    public interface IEmberConverter
    {
        /// <summary>Reads the EmBER-encoded data in <paramref name="buffer"/> and writes an equivalent XML
        /// representation with <paramref name="writer"/>.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> and/or <paramref name="writer"/> equal
        /// <c>null</c>.</exception>
        /// <exception cref="EmberException">The EmBER-encoded data is invalid, see <see cref="Exception.Message"/> for
        /// details.</exception>
        void ToXml(byte[] buffer, XmlWriter writer);
    }
}
