////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.IO
{
    /// <summary>Provides common defaults.</summary>
    /// <threadsafety static="true" instance="false"/>
    public static class Defaults
    {
        /// <summary>The default size of the temporary in-memory buffer that is allocated to read/write from a stream
        /// that retrieves/sends its data from/to memory.</summary>
        public const int InMemoryStreamBufferSize = 1024;

        /// <summary>The default size of the temporary in-memory buffer that is allocated to read from/write to a stream
        /// that is presumed to send/receive its data directly to/from a physical resource, like a network or a file on
        /// a disk.</summary>
        public const int PhysicalStreamBufferSize = 8192;
    }
}
