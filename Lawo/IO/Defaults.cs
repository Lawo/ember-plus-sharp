////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.IO
{
    /// <summary>Provides common defaults.</summary>
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
