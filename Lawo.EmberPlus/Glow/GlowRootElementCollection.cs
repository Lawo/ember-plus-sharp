////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Glow
{
    using System.Diagnostics.CodeAnalysis;

    using Ember;

    internal static class GlowRootElementCollection
    {
        internal const int InnerNumber = Ember.InnerNumber.FirstApplication + 11;
        internal const string Name = "RootElementCollection";

        internal static class RootElement
        {
            internal const int OuterNumber = 0;
            internal const string Name = "RootElement";

            [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Used through reflection.")]
            internal static readonly EmberId OuterId = EmberId.CreateContextSpecific(OuterNumber);
        }
    }
}
