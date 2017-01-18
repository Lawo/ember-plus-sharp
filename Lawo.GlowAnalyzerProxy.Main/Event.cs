////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.GlowAnalyzerProxy.Main
{
    using System.Diagnostics.CodeAnalysis;

    internal sealed class Event
    {
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called through reflection.")]
        public int Conn { get; }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called through reflection.")]
        public double Time { get; }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called through reflection.")]
        public string Type { get; }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called through reflection.")]
        public string Dir { get; }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called through reflection.")]
        public int? No { get; }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called through reflection.")]
        public int? Payload { get; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal Event(
            int connection,
            double time,
            string type,
            string direction,
            int? number,
            int? length,
            string logPath,
            long logPosition,
            long logLength)
        {
            this.Conn = connection;
            this.Time = time;
            this.Type = type;
            this.Dir = direction;
            this.No = number;
            this.Payload = length;
            this.LogPath = logPath;
            this.LogPosition = logPosition;
            this.LogLength = logLength;
        }

        internal string LogPath { get; }

        internal long LogPosition { get; }

        internal long LogLength { get; }
    }
}
