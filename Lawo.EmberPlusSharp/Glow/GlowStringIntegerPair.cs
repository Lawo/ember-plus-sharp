////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Glow
{
    using Ember;

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
