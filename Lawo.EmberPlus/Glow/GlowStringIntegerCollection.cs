////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Glow
{
    using System.Diagnostics.CodeAnalysis;

    using Lawo.EmberPlus.Ember;

    internal static class GlowStringIntegerCollection
    {
        internal const int InnerNumber = Ember.InnerNumber.FirstApplication + 8;
        internal const string Name = "StringIntegerCollection";

        internal static class StringIntegerPair
        {
            internal const int OuterNumber = 0;
            internal const string Name = "StringIntegerPair";

            [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Used through reflection.")]
            internal static readonly EmberId OuterId = EmberId.CreateContextSpecific(OuterNumber);
        }
    }
}
