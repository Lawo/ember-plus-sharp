////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.S101
{
    using System;
    using System.Globalization;
    using System.Xml;

    using Ember;

    /// <summary>Reads the information written by a <see cref="S101Logger"/> instance.</summary>
    /// <threadsafety static="true" instance="false"/>
    public sealed class S101LogReader
    {
        /// <summary>Initializes a new instance of the <see cref="S101LogReader"/> class.</summary>
        /// <param name="types">The types to pass to the internal <see cref="EmberConverter"/>, which is used to convert
        /// between XML payload and EmBER payload.</param>
        /// <param name="logReader">The <see cref="XmlReader"/> to read the events from. The format needs to match the
        /// one written by <see cref="S101Logger"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="types"/> and/or <paramref name="logReader"/> equal
        /// <c>null</c>.</exception>
        public S101LogReader(EmberTypeBag types, XmlReader logReader)
        {
            this.converter = new EmberConverter(types);
            this.logReader = logReader ?? throw new ArgumentNullException(nameof(logReader));
            this.logReader.ReadStartElement(LogNames.Root);
        }

        /// <summary>Gets the type of the current event.</summary>
        /// <value>Either <c>Message</c> or <c>OutOfFrameByte</c>.</value>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item><see cref="Read"/> has never been called, or</item>
        /// <item>the last call to <see cref="Read"/> returned <c>false</c> or threw an exception.</item>
        /// </list></exception>
        public string EventType
        {
            get
            {
                this.AssertRead();
                return this.eventType;
            }
        }

        /// <summary>Gets the UTC time of the current event.</summary>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item><see cref="Read"/> has never been called, or</item>
        /// <item>the last call to <see cref="Read"/> returned <c>false</c> or threw an exception.</item>
        /// </list></exception>
        public DateTime TimeUtc
        {
            get
            {
                this.AssertRead();
                return this.timeUtc;
            }
        }

        /// <summary>Gets the direction of the current event.</summary>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item><see cref="Read"/> has never been called, or</item>
        /// <item>the last call to <see cref="Read"/> returned <c>false</c> or threw an exception.</item>
        /// </list></exception>
        public string Direction
        {
            get
            {
                this.AssertRead();
                return this.direction;
            }
        }

        /// <summary>Gets the number of the current event.</summary>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item><see cref="Read"/> has never been called, or</item>
        /// <item>the last call to <see cref="Read"/> returned <c>false</c> or threw an exception.</item>
        /// </list></exception>
        public int Number
        {
            get
            {
                this.AssertRead();
                return this.number;
            }
        }

        /// <summary>Gets the current message.</summary>
        /// <value><c>null</c>, if <see cref="Type"/> is not equal to <c>"Message"</c>.</value>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item><see cref="Read"/> has never been called, or</item>
        /// <item>the last call to <see cref="Read"/> returned <c>false</c> or threw an exception.</item>
        /// </list></exception>
        public S101Message Message
        {
            get
            {
                this.AssertRead();
                return this.message;
            }
        }

        /// <summary>Reads the next event.</summary>
        /// <returns><c>true</c> if the next event was read successfully; <c>false</c> if there are no more events
        /// to read.</returns>
        /// <exception cref="XmlException">An error occurred while parsing the XML-encoded data, see
        /// <see cref="Exception.Message"/> for more information.</exception>
        /// <remarks>
        /// <para>When a <see cref="S101LogReader"/> is first created and initialized, there is no information
        /// available. You must call <see cref="Read"/> to read the first event.</para></remarks>
        public bool Read()
        {
            try
            {
                return this.ReadCore();
            }
            catch
            {
                this.Clear();
                throw;
            }
        }

        /// <summary>Gets the payload of the current event.</summary>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item><see cref="Read"/> has never been called, or</item>
        /// <item>the last call to <see cref="Read"/> returned <c>false</c> or threw an exception.</item>
        /// </list></exception>
        public byte[] GetPayload()
        {
            this.AssertRead();
            return this.payload;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal EmberConverter Converter => this.converter;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static readonly byte[] NoPayload = new byte[0];

        private static byte ParseHex(string str) =>
            byte.Parse(str, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);

        private readonly EmberConverter converter;
        private readonly XmlReader logReader;
        private string eventType;
        private DateTime timeUtc;
        private string direction;
        private int number;
        private S101Message message;
        private byte[] payload;

        private bool ReadCore()
        {
            while (this.logReader.IsStartElement(LogNames.Event))
            {
                switch (this.eventType = this.logReader.GetAttribute(LogNames.Type))
                {
                    case LogNames.Message:
                        this.timeUtc = this.ReadTime();
                        this.direction = this.logReader.GetAttribute(LogNames.Direction);
                        this.number = int.Parse(
                            this.logReader.GetAttribute(LogNames.Number),
                            NumberStyles.None,
                            CultureInfo.InvariantCulture);
                        this.logReader.ReadStartElement(LogNames.Event);
                        this.logReader.ReadStartElement(LogNames.Slot);
                        var slot = ParseHex(this.logReader.ReadContentAsString());
                        this.logReader.ReadEndElement();
                        this.logReader.ReadStartElement(LogNames.Command);
                        var command = S101Command.Parse(this.logReader.ReadContentAsString());
                        this.logReader.ReadEndElement();
                        this.message = new S101Message(slot, command);

                        if (!this.logReader.IsStartElement(LogNames.Payload))
                        {
                            throw new XmlException("The Payload element is missing.");
                        }

                        this.payload = this.GetLogPayload();
                        return true;
                    case LogNames.OutOfFrameByte:
                        this.timeUtc = this.ReadTime();
                        this.direction = this.logReader.GetAttribute(LogNames.Direction);
                        this.number = 0;
                        this.message = null;
                        this.logReader.ReadStartElement(LogNames.Event);
                        this.payload = new[] { ParseHex(this.logReader.ReadContentAsString()) };
                        this.logReader.ReadEndElement();
                        return true;
                    default:
                        this.logReader.Skip();
                        break;
                }
            }

            this.logReader.ReadEndElement();
            this.Clear();
            return false;
        }

        private DateTime ReadTime()
        {
            try
            {
                return DateTime.ParseExact(
                    this.logReader.GetAttribute(LogNames.Time),
                    "HH':'mm':'ss'.'ff",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
            }
            catch (ArgumentNullException)
            {
                return DateTime.Today;
            }
            catch (FormatException)
            {
                return DateTime.Today;
            }
        }

        private byte[] GetLogPayload()
        {
            byte[] result;

            if (this.logReader.IsEmptyElement)
            {
                result = NoPayload;
                this.logReader.Skip();
            }
            else
            {
                this.logReader.ReadStartElement(LogNames.Payload);

                if (this.logReader.IsStartElement())
                {
                    using (var reader = this.logReader.ReadSubtree())
                    {
                        result = this.converter.FromXml(reader);
                    }

                    this.logReader.ReadEndElement();
                }
                else
                {
                    result = NoPayload;
                }

                this.logReader.ReadEndElement();
            }

            this.logReader.ReadEndElement();
            return result;
        }

        private void AssertRead()
        {
            if (this.eventType == null)
            {
                throw new InvalidOperationException(
                    "Read() has never been called, the last call to Read() returned false or threw an exception.");
            }
        }

        private void Clear()
        {
            this.eventType = null;
            this.timeUtc = DateTime.Today;
            this.direction = null;
            this.number = 0;
            this.message = null;
            this.payload = null;
        }
    }
}
