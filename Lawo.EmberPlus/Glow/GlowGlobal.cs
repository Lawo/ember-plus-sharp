////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Glow
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
