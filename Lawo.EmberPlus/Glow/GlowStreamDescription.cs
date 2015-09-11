////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Glow
{
    using Ember;

    internal static class GlowStreamDescription
    {
        internal const int InnerNumber = Ember.InnerNumber.FirstApplication + 12;
        internal const string Name = "StreamDescription";

        internal static class Format
        {
            internal const int OuterNumber = 0;
            internal const string Name = "format";
            internal static readonly EmberId OuterId = EmberId.CreateContextSpecific(OuterNumber);
        }

        internal static class Offset
        {
            internal const int OuterNumber = 1;
            internal const string Name = "offset";
            internal static readonly EmberId OuterId = EmberId.CreateContextSpecific(OuterNumber);
        }
    }
}
