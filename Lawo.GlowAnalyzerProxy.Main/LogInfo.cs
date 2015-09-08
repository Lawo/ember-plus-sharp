////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.GlowAnalyzerProxy.Main
{
    using System;
    using System.IO;
    using System.Xml;
    using Lawo.EmberPlus.Glow;
    using Lawo.EmberPlus.S101;

    internal sealed class LogInfo : IDisposable
    {
        private readonly DateTime startTimeUtc = DateTime.UtcNow;
        private readonly string path;
        private readonly Stream stream;
        private readonly StreamWriter writer;
        private readonly XmlWriter xmlWriter;
        private readonly S101Logger logger;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

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

        internal DateTime StartTimeUtc
        {
            get { return this.startTimeUtc; }
        }

        internal string Path
        {
            get { return this.path; }
        }

        internal long StartPosition
        {
            get
            {
                // The > of the <S101Log> tag is only written when the first element of its content is written.
                // For subsequent elements the full closing tag of the previous element has already been written.
                // For both cases, the CRLF before the next tag has not been written yet.
                return this.writer.BaseStream.Position + (this.xmlWriter.WriteState == WriteState.Element ? 3 : 2);
            }
        }

        internal long? EndPosition
        {
            get { return this.writer.BaseStream == null ? (long?)null : this.writer.BaseStream.Position; }
        }

        internal S101Logger Logger
        {
            get { return this.logger; }
        }
    }
}
