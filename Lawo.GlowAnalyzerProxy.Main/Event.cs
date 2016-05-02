////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2016 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.GlowAnalyzerProxy.Main
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Lawo.EmberPlusSharp.S101;

    internal sealed class Event
    {
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called through reflection.")]
        public int Conn
        {
            get { return this.conn; }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called through reflection.")]
        public double Time
        {
            get { return this.time; }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called through reflection.")]
        public string Type
        {
            get { return this.type; }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called through reflection.")]
        public string Dir
        {
            get { return this.direction; }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called through reflection.")]
        public int? No
        {
            get { return this.number; }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called through reflection.")]
        public int? Payload
        {
            get { return this.length; }
        }

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
            this.conn = connection;
            this.time = time;
            this.type = type;
            this.direction = direction;
            this.number = number;
            this.length = length;
            this.logPath = logPath;
            this.logPosition = logPosition;
            this.logLength = logLength;
        }

        internal string LogPath
        {
            get { return this.logPath; }
        }

        internal long LogPosition
        {
            get { return this.logPosition; }
        }

        internal long LogLength
        {
            get { return this.logLength; }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private readonly int conn;
        private readonly double time;
        private readonly string type;
        private readonly string direction;
        private readonly int? number;
        private readonly int? length;
        private readonly string logPath;
        private readonly long logPosition;
        private readonly long logLength;
    }
}
