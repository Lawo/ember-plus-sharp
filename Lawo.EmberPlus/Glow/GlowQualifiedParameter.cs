﻿////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Glow
{
    using System.Diagnostics.CodeAnalysis;

    using Ember;

    internal static class GlowQualifiedParameter
    {
        internal const int InnerNumber = Ember.InnerNumber.FirstApplication + 9;
        internal const string Name = "QualifiedParameter";

        internal static class Path
        {
            internal const int OuterNumber = 0;
            internal const string Name = "path";
            internal static readonly EmberId OuterId = EmberId.CreateContextSpecific(OuterNumber);
        }

        internal static class Contents
        {
            internal const int OuterNumber = 1;
            internal const string Name = "contents";
            internal static readonly EmberId OuterId = EmberId.CreateContextSpecific(OuterNumber);
        }

        internal static class Children
        {
            internal const int OuterNumber = 2;
            internal const string Name = "children";

            [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Used through reflection.")]
            internal static readonly EmberId OuterId = EmberId.CreateContextSpecific(OuterNumber);
        }
    }
}