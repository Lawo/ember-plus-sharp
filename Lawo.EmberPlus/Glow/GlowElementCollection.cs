////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Glow
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
