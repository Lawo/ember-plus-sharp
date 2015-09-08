////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model.Test
{
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated through reflection.")]
    internal sealed class Ravenna : FieldNode<Ravenna>
    {
        internal CollectionNode<MediaSession> MediaSessions { get; private set; }

        [Element(IsOptional = true)]
        internal CollectionNode<MediaSessionReceiver> MediaSessionReceivers { get; private set; }
    }
}
