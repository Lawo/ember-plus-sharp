////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Glow
{
    using System.Diagnostics.CodeAnalysis;

    using Lawo.EmberPlus.Ember;

    internal static class GlowStringIntegerPair
    {
        internal const int InnerNumber = Ember.InnerNumber.FirstApplication + 7;
        internal const string Name = "StringIntegerPair";

        internal static class EntryString
        {
            internal const int OuterNumber = 0;
            internal const string Name = "entryString";
            internal static readonly EmberId OuterId = EmberId.CreateContextSpecific(OuterNumber);
        }

        internal static class EntryInteger
        {
            internal const int OuterNumber = 1;
            internal const string Name = "entryInteger";
            internal static readonly EmberId OuterId = EmberId.CreateContextSpecific(OuterNumber);
        }
    }
}
