////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Glow
{
    using System.Diagnostics.CodeAnalysis;

    using Ember;

    internal static class GlowStreamCollection
    {
        internal const int InnerNumber = Ember.InnerNumber.FirstApplication + 6;
        internal const string Name = "StreamCollection";

        internal static class StreamEntry
        {
            internal const int OuterNumber = 0;
            internal const string Name = "StreamEntry";

            [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Used through reflection.")]
            internal static readonly EmberId OuterId = EmberId.CreateContextSpecific(OuterNumber);
        }
    }
}
