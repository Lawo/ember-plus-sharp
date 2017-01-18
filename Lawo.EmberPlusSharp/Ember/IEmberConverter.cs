////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Ember
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
