////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Glow
{
    using Ember;

    internal static class GlowElementCollection
    {
        internal const int InnerNumber = Ember.InnerNumber.FirstApplication + 4;
        internal const string Name = "ElementCollection";

        internal static class Element
        {
            internal const int OuterNumber = 0;
            internal const string Name = "Element";
            internal static readonly EmberId OuterId = EmberId.CreateContextSpecific(OuterNumber);
        }
    }
}
