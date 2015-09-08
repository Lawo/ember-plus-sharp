////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Glow
{
    using System.Diagnostics.CodeAnalysis;

    using Lawo.EmberPlus.Ember;

    internal static class GlowTupleItemDescription
    {
        internal const int InnerNumber = Ember.InnerNumber.FirstApplication + 21;
        internal const string Name = "TupleItemDescription";

        internal static class Type
        {
            internal const int OuterNumber = 0;
            internal const string Name = "type";

            [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Used through reflection.")]
            internal static readonly EmberId OuterId = EmberId.CreateContextSpecific(OuterNumber);
        }

        internal static class TheName
        {
            internal const int OuterNumber = 1;
            internal const string Name = "name";

            [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Used through reflection.")]
            internal static readonly EmberId OuterId = EmberId.CreateContextSpecific(OuterNumber);
        }
    }
}
