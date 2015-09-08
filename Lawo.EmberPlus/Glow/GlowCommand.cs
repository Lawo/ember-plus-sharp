////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Glow
{
    using System.Diagnostics.CodeAnalysis;

    using Lawo.EmberPlus.Ember;

    internal static class GlowCommand
    {
        internal const int InnerNumber = Ember.InnerNumber.FirstApplication + 2;
        internal const string Name = "Command";

        internal static class Number
        {
            internal const int OuterNumber = 0;
            internal const string Name = "number";
            internal static readonly EmberId OuterId = EmberId.CreateContextSpecific(OuterNumber);
        }

        internal static class DirFieldMask
        {
            internal const int OuterNumber = 1;
            internal const string Name = "dirFieldMask";

            [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Used through reflection.")]
            internal static readonly EmberId OuterId = EmberId.CreateContextSpecific(OuterNumber);
        }

        internal static class Invocation
        {
            internal const int OuterNumber = 2;
            internal const string Name = "invocation";

            [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Used through reflection.")]
            internal static readonly EmberId OuterId = EmberId.CreateContextSpecific(OuterNumber);
        }
    }
}
