////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Glow
{
    using System.Diagnostics.CodeAnalysis;

    using Ember;

    internal static class GlowLabel
    {
        internal const int InnerNumber = Ember.InnerNumber.FirstApplication + 18;
        internal const string Name = "Label";

        internal static class BasePath
        {
            internal const int OuterNumber = 0;
            internal const string Name = "basePath";

            [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Used through reflection.")]
            internal static readonly EmberId OuterId = EmberId.CreateContextSpecific(OuterNumber);
        }

        internal static class Description
        {
            internal const int OuterNumber = 1;
            internal const string Name = "description";

            [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Used through reflection.")]
            internal static readonly EmberId OuterId = EmberId.CreateContextSpecific(OuterNumber);
        }
    }
}
