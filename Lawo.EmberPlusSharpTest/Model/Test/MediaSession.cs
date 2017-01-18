////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model.Test
{
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated through reflection.")]
    internal sealed class MediaSession : FieldNode<MediaSession>
    {
        [Element(Identifier = "name")]
        internal StringParameter Name { get; private set; }

        [Element(Identifier = "id")]
        internal StringParameter Id { get; private set; }

        [Element(Identifier = "state")]
        internal IntegerParameter State { get; private set; }
    }
}
