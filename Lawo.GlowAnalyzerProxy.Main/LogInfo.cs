////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.GlowAnalyzerProxy.Main
{
    using System;
    using System.IO;
    using System.Xml;
    using Lawo.EmberPlusSharp.Glow;
    using Lawo.EmberPlusSharp.S101;

    internal sealed class LogInfo : IDisposable
    {
        public void Dispose()
        {
            this.logger.Dispose();
            this.xmlWriter.Dispose();
            this.writer.Dispose();
            this.stream.Dispose();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal LogInfo(string path)
        {
            this.path = path;
            this.stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
            this.writer = new StreamWriter(this.stream);
            this.xmlWriter = XmlWriter.Create(this.writer, new XmlWriterSettings { Indent = true, CloseOutput = true });
            this.logger = new S101Logger(GlowTypes.Instance, this.xmlWriter);
        }

        internal DateTime StartTimeUtc => this.startTimeUtc;

        internal string Path => this.path;

        // The > of the <S101Log> tag is only written when the first element of its content is written.
        // For subsequent elements the full closing tag of the previous element has already been written.
        // For both cases, the CRLF before the next tag has not been written yet.
        internal long StartPosition =>
            this.writer.BaseStream.Position + (this.xmlWriter.WriteState == WriteState.Element ? 3 : 2);

        internal long? EndPosition => this.writer.BaseStream == null ? (long?)null : this.writer.BaseStream.Position;

        internal S101Logger Logger => this.logger;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private readonly DateTime startTimeUtc = DateTime.UtcNow;
        private readonly string path;
        private readonly Stream stream;
        private readonly StreamWriter writer;
        private readonly XmlWriter xmlWriter;
        private readonly S101Logger logger;
    }
}
