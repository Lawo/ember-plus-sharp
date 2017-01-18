////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Glow
{
    using Ember;

    internal static class GlowGlobal
    {
        internal const int InnerNumber = EmberGlobal.InnerNumber;
        internal const string Name = EmberGlobal.Name;

        internal static class Root
        {
            internal const int OuterNumber = 0;
            internal const string Name = "Root";
            internal static readonly EmberId OuterId = EmberId.CreateApplication(OuterNumber);
        }
    }
}
