////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.S101
{
    internal static class Constants
    {
        // See http://ember-plus.googlecode.com/svn/trunk/documentation/Ember+%20Documentation.pdf
        internal const int MessageHeaderMaxLength = 9;

        /// <summary>The default size of the temporary in-memory buffer that is allocated to read from/write to a stream
        /// that is presumed to send/receive its data directly to/from a physical resource, like a network or a file on
        /// a disk.</summary>
        internal const int PhysicalStreamBufferSize = 8192;
    }
}
