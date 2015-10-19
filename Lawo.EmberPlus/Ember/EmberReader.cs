////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Ember
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;

    using IO;

    /// <summary>Represents a reader that provides the means to read BER-encoded input, as specified in
    /// <i>"X.690"</i><cite>X.690</cite>.</summary>
    /// <remarks>Only the subset defined in the <i>"Ember+ Specification"</i><cite>Ember+ Specification</cite> is
    /// supported.</remarks>
    /// <threadsafety static="true" instance="false"/>
    public sealed class EmberReader : IDisposable
    {
        private static readonly EmberId EndContainer = EmberId.CreateUniversal(Ember.InnerNumber.EndContainer);
        private static readonly EmberId Sequence = EmberId.CreateUniversal(Ember.InnerNumber.Sequence);
        private static readonly EmberId Set = EmberId.CreateUniversal(Ember.InnerNumber.Set);

        private readonly Stack<PositionInfo> endPositions = new Stack<PositionInfo>(32);
        private Stream stream;
        private readonly ReadBuffer readBuffer;
        private int? innerNumber;
        private EmberId? outer;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Initializes a new instance of the <see cref="EmberReader"/> class by calling
        /// <see cref="EmberReader(Stream, int)">EmberReader(<paramref name="stream"/>, 1024)</see>.</summary>
        public EmberReader(Stream stream) : this(stream, Constants.MemoryStreamBufferSize)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="EmberReader"/> class.</summary>
        /// <param name="stream">The stream from which the EmBER-encoded input will be read.</param>
        /// <param name="bufferSize">The size of the internal buffer in bytes.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> equals <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="bufferSize"/> is 0 or negative.</exception>
        public EmberReader(Stream stream, int bufferSize)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            this.stream = stream;
            this.readBuffer = new ReadBuffer((ReadCallback)this.stream.Read, bufferSize);
        }

        /// <summary>Releases all resources used by the current instance of the <see cref="EmberWriter"/> class.</summary>
        /// <remarks>Calls <see cref="Stream.Dispose()"/> on the stream passed to the constructor.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Dispose() must never throw.")]
        public void Dispose()
        {
            try
            {
                if (this.stream != null)
                {
                    this.stream.Dispose();
                }
            }
            catch
            {
            }
            finally
            {
                this.stream = null;
                this.CanReadContents = false;
            }
        }

        /// <summary>Gets the number of the inner identifier of the data value that was read with <see cref="Read"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item><see cref="Read"/> has never been called, or</item>
        /// <item>The last <see cref="Read"/> call returned <c>false</c> or threw an exception.</item>
        /// </list></exception>
        /// <exception cref="ObjectDisposedException"><see cref="Dispose"/> has been called.</exception>
        /// <remarks>The number of an Universal class identifier is returned unaltered.
        /// <see cref="InnerNumber.FirstApplication"/> is added to the number of an Application class identifier.</remarks>
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "There's not much of an alternative.")]
        public int InnerNumber
        {
            get
            {
                this.AssertRead();
                return this.innerNumber.Value;
            }
        }

        /// <summary>Gets the outer identifier of the data value that was read with <see cref="Read"/>.</summary>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item><see cref="Read"/> has never been called, or</item>
        /// <item>The last <see cref="Read"/> call returned <c>false</c> or threw an exception, or</item>
        /// <item>The value of <see cref="InnerNumber"/> currently equals <see cref="InnerNumber.EndContainer"/>.</item>
        /// </list></exception>
        /// <exception cref="ObjectDisposedException"><see cref="Dispose"/> has been called.</exception>
        public EmberId OuterId
        {
            get
            {
                this.AssertRead();

                if (!this.outer.HasValue)
                {
                    throw new InvalidOperationException("The current data value does not have an outer identifier.");
                }

                return this.outer.Value;
            }
        }

        /// <summary>Gets a value indicating whether contents can be read with one of the ReadContents methods.
        /// </summary>
        /// <value><c>true</c>, if all of the following conditions hold; otherwise, <c>false</c>:
        /// <list type="bullet">
        /// <item>The last call to <see cref="Read"/> has returned <c>true</c>, and</item>
        /// <item>The <see cref="InnerNumber"/> property equals <see cref="InnerNumber.Boolean"/>,
        /// <see cref="InnerNumber.Integer"/>, <see cref="InnerNumber.Octetstring"/>,
        /// <see cref="InnerNumber.Real"/>, <see cref="InnerNumber.Utf8String"/> or
        /// <see cref="InnerNumber.RelativeObjectIdentifier"/>, and</item>
        /// <item>None of the ReadContents methods have yet been called for the current data value.</item>
        /// </list></value>
        public bool CanReadContents { get; private set; }

        /// <summary>Advances the reader to the next data value in the stream.</summary>
        /// <returns><c>true</c> if the operation completed successfully; <c>false</c> if the end of the stream has
        /// been reached.</returns>
        /// <exception cref="EmberException">An error occurred while parsing the EmBER-encoded data, see
        /// <see cref="Exception.Message"/> for more information.</exception>
        /// <exception cref="ObjectDisposedException"><see cref="Dispose"/> has been called.</exception>
        /// <remarks>
        /// <para>After this method returns <c>true</c>, client code usually examines the values of the
        /// <see cref="EmberReader.InnerNumber"/> and <see cref="OuterId"/> properties to determine the next steps. This
        /// method returns <c>true</c> in the following situations:
        /// <list type="bullet">
        /// <item>The identifiers and lengths of a data value with primitive encoding have been read successfully. Call
        /// the appropriate ReadContents method to retrieve the contents of the data value.</item>
        /// <item>The identifiers and lengths of a sequence, a set or an application-defined type have been read
        /// successfully. Call <see cref="Read"/> to advance the reader to the data values of the container.</item>
        /// <item>The reader has read past the end of a sequence, a set or an application-defined type with definite
        /// length. Call <see cref="Read"/> to advance the reader to the data values located after the container.</item>
        /// <item>The End-of-contents marker of a sequence, a set or an application-defined type with indefinite length
        /// has been read successfully. Call <see cref="Read"/> to advance the reader to the data values located after
        /// the container.</item>
        /// </list></para>
        /// <para>When a <see cref="EmberReader"/> is first created and initialized, there is no information available.
        /// You must call <see cref="Read"/> to read the first data value.</para>
        /// <para>Possibly unread contents of the previous data value with primitive encoding is skipped automatically.
        /// </para>
        /// </remarks>
        public bool Read()
        {
            this.AssertNotDisposed();

            if (this.CanReadContents)
            {
                var endPosition = this.EndPosition.Value;

                while ((this.readBuffer.Position < endPosition) &&
                    ((this.readBuffer.Index < this.readBuffer.Count) || this.readBuffer.Read()))
                {
                    this.readBuffer.Index +=
                        (int)Math.Min(this.readBuffer.Count - this.readBuffer.Index, endPosition - this.readBuffer.Position);
                }

                this.CanReadContents = false;
            }

            this.innerNumber = null;
            this.outer = null;

            if ((this.endPositions.Count > 0) && (this.readBuffer.Position > this.EndPosition))
            {
                throw CreateEmberException("Incorrect length at position {0}.", this.endPositions.Peek().LengthPosition);
            }

            while ((this.endPositions.Count > 0) && (this.readBuffer.Position == this.EndPosition))
            {
                var endPosition = this.endPositions.Pop();

                if (IsContainer(endPosition))
                {
                    this.innerNumber = Ember.InnerNumber.EndContainer;
                    return true;
                }
            }

            return this.ReadCore();
        }

        /// <summary>Reads the contents of the current data value as a <see cref="bool"/>.</summary>
        /// <exception cref="EmberException">An error occurred while parsing the EmBER-encoded data, see
        /// <see cref="Exception.Message"/> for more information.</exception>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item>The value of <see cref="InnerNumber"/> is not equal to <see cref="InnerNumber.Boolean"/>, or</item>
        /// <item>The contents of the current data value has already been read.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ObjectDisposedException"><see cref="Dispose"/> has been called.</exception>
        public bool ReadContentsAsBoolean()
        {
            this.AssertCanReadContents(Ember.InnerNumber.Boolean);
            return Read8Bit(this.readBuffer, this.ContentsLength.Value, false) != 0;
        }

        /// <summary>Reads the contents of the current data value as a <see cref="long"/>.</summary>
        /// <exception cref="EmberException">An error occurred while parsing the EmBER-encoded data, see
        /// <see cref="Exception.Message"/> for more information.</exception>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item>The value of <see cref="InnerNumber"/> is not equal to <see cref="InnerNumber.Integer"/>, or</item>
        /// <item>The contents of the current data value has already been read.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ObjectDisposedException"><see cref="Dispose"/> has been called.</exception>
        public long ReadContentsAsInt64()
        {
            this.AssertCanReadContents(Ember.InnerNumber.Integer);
            return Read8Bit(this.readBuffer, this.ContentsLength.Value, true);
        }

        /// <summary>Reads the contents of the current data value as a <see cref="byte"/> array.</summary>
        /// <exception cref="EmberException">An error occurred while parsing the EmBER-encoded data, see
        /// <see cref="Exception.Message"/> for more information.</exception>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item>The value of <see cref="InnerNumber"/> is not equal to <see cref="InnerNumber.Octetstring"/>,
        /// or</item>
        /// <item>The contents of the current data value has already been read.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ObjectDisposedException"><see cref="Dispose"/> has been called.</exception>
        public byte[] ReadContentsAsByteArray()
        {
            this.AssertCanReadContents(Ember.InnerNumber.Octetstring);
            var result = new byte[this.ContentsLength.Value];

            try
            {
                this.readBuffer.Fill(result, 0, result.Length);
            }
            catch (EndOfStreamException ex)
            {
                throw CreateEmberException(ex);
            }

            return result;
        }

        /// <summary>Reads the contents of the current data value as a <see cref="double"/>.</summary>
        /// <exception cref="EmberException">An error occurred while parsing the EmBER-encoded data, see
        /// <see cref="Exception.Message"/> for more information.</exception>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item>The value of <see cref="InnerNumber"/> is not equal to <see cref="InnerNumber.Real"/>, or</item>
        /// <item>The contents of the current data value has already been read.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ObjectDisposedException"><see cref="Dispose"/> has been called.</exception>
        public double ReadContentsAsDouble()
        {
            this.AssertCanReadContents(Ember.InnerNumber.Real);
            return ReadReal(this.readBuffer, this.ContentsLength.Value);
        }

        /// <summary>Reads the contents of the current data value as a <see cref="string"/>.</summary>
        /// <exception cref="EmberException">An error occurred while parsing the EmBER-encoded data, see
        /// <see cref="Exception.Message"/> for more information.</exception>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item>The value of <see cref="InnerNumber"/> is not equal to <see cref="InnerNumber.Utf8String"/>,
        /// or</item>
        /// <item>The contents of the current data value has already been read.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ObjectDisposedException"><see cref="Dispose"/> has been called.</exception>
        public string ReadContentsAsString()
        {
            this.AssertCanReadContents(Ember.InnerNumber.Utf8String);
            return this.readBuffer.ReadUtf8(this.ContentsLength.Value);
        }

        /// <summary>Reads the contents of the current data value as an <see cref="int"/> array.</summary>
        /// <exception cref="EmberException">An error occurred while parsing the EmBER-encoded data, see
        /// <see cref="Exception.Message"/> for more information.</exception>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item>The value of <see cref="InnerNumber"/> is not equal to
        /// <see cref="InnerNumber.RelativeObjectIdentifier"/>, or</item>
        /// <item>The contents of the current data value has already been read.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ObjectDisposedException"><see cref="Dispose"/> has been called.</exception>
        public int[] ReadContentsAsInt32Array()
        {
            this.AssertCanReadContents(Ember.InnerNumber.RelativeObjectIdentifier);
            var endPosition = this.EndPosition.Value;
            var result = new List<int>();

            while (this.readBuffer.Position < endPosition)
            {
                result.Add(Read7Bit(this.readBuffer));
            }

            return result.ToArray();
        }

        /// <summary>Reads the contents of the current data value.</summary>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item>The value of <see cref="CanReadContents"/> equals <c>false</c>, or</item>
        /// <item>One of the ReadContents methods has already been called for the current data value.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ObjectDisposedException"><see cref="Dispose"/> has been called.</exception>
        public object ReadContentsAsObject()
        {
            switch (this.innerNumber)
            {
                case Ember.InnerNumber.Boolean:
                    return this.ReadContentsAsBoolean();
                case Ember.InnerNumber.Integer:
                    return this.ReadContentsAsInt64();
                case Ember.InnerNumber.Octetstring:
                    return this.ReadContentsAsByteArray();
                case Ember.InnerNumber.Real:
                    return this.ReadContentsAsDouble();
                case Ember.InnerNumber.Utf8String:
                    return this.ReadContentsAsString();
                case Ember.InnerNumber.RelativeObjectIdentifier:
                    return this.ReadContentsAsInt32Array();
                default:
                    this.AssertRead();
                    throw new InvalidOperationException(
                        "The current data value does not have a contents with primitive encoding.");
            }
        }

        /// <summary>Skips the contents of the current data value.</summary>
        /// <exception cref="InvalidOperationException">
        /// <list type="bullet">
        /// <item><see cref="Read"/> has never been called, or</item>
        /// <item>The last <see cref="Read"/> call returned <c>false</c> or threw an exception.</item>
        /// </list></exception>
        /// <exception cref="ObjectDisposedException"><see cref="Dispose"/> has been called.</exception>
        /// <remarks>
        /// <para>If the <see cref="EmberReader"/> instance is currently placed on the start of a container, then skips
        /// to the end of the container, such that calling <see cref="Read"/> afterwards will place the reader on either
        /// a sibling of the container, the end of the parent container or the end of the stream.</para>
        /// <para>This method has no effect, if the reader is currently placed on a data value with primitive encoding
        /// (the next call to <see cref="Read"/> will skip possibly unread contents anyway).</para>
        /// </remarks>
        public void Skip()
        {
            this.SkipCore(this.InnerNumber);
        }

        /// <summary>Skips to the end of the current container.</summary>
        /// <exception cref="ObjectDisposedException"><see cref="Dispose"/> has been called.</exception>
        /// <remarks>
        /// <para>While <see cref="Read"/> returns <c>true</c> and <see cref="InnerNumber"/> is not equal to
        /// <see cref="Ember.InnerNumber.EndContainer"/>, calls <see cref="Skip"/>.</para>
        /// </remarks>
        public void SkipToEndContainer()
        {
            int inner;

            while (this.Read() && ((inner = this.innerNumber.Value) != Ember.InnerNumber.EndContainer))
            {
                this.SkipCore(inner);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private long? EndPosition
        {
            get { return this.endPositions.Peek().EndPosition; }
        }

        private int? ContentsLength
        {
            get { return (int?)(this.EndPosition - this.readBuffer.Position); }
        }

        private void AssertNotDisposed()
        {
            if (this.stream == null)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }

        private void AssertRead()
        {
            this.AssertNotDisposed();

            if (!this.innerNumber.HasValue)
            {
                throw new InvalidOperationException("Read() has either never been called or " +
                    "the last call to Read() returned false or threw an exception.");
            }
        }

        private void AssertCanReadContents(int expectedInnerNumber)
        {
            this.AssertNotDisposed();

            if (!this.innerNumber.HasValue || (this.innerNumber.Value != expectedInnerNumber))
            {
                throw new InvalidOperationException("The current data value does not have contents of the requested type.");
            }

            if (!this.CanReadContents)
            {
                throw new InvalidOperationException("The contents of the current data value has already been read.");
            }

            this.CanReadContents = false;
        }

        private bool ReadCore()
        {
            try
            {
                while ((this.readBuffer.Index < this.readBuffer.Count) || this.readBuffer.Read())
                {
                    var outerIdentifierPosition = this.readBuffer.Position;
                    var outerIdentifier = ReadIdentifier(this.readBuffer);

                    if (this.ProcessOuter(outerIdentifier, outerIdentifierPosition))
                    {
                        if (outerIdentifier == EndContainer)
                        {
                            this.innerNumber = Ember.InnerNumber.EndContainer;
                            return true;
                        }

                        if (outerIdentifier.Class == Class.Universal)
                        {
                            throw CreateEmberException(
                                "Unexpected Universal class for outer identifier at position {0}.",
                                outerIdentifierPosition);
                        }

                        this.ReadAndProcessInner();
                        this.outer = outerIdentifier;
                        return true;
                    }
                }
            }
            catch (EndOfStreamException ex)
            {
                throw CreateEmberException(ex);
            }

            if (this.endPositions.Count > 0)
            {
                throw CreateEmberException("Unexpected end of stream at position {0}.", this.readBuffer.Position);
            }

            return false;
        }

        private void SkipCore(int inner)
        {
            if ((inner >= Ember.InnerNumber.FirstApplication) ||
                (inner == Ember.InnerNumber.Sequence) || (inner == Ember.InnerNumber.Set))
            {
                this.SkipToEndContainer();
            }
        }

        private bool ProcessOuter(EmberId id, long idPosition)
        {
            if (id == EndContainer)
            {
                if (ReadLength(this.readBuffer) != 0)
                {
                    throw CreateEmberException(
                        "Unexpected length for End-of-contents identifier at position {0}.", idPosition);
                }

                if (this.endPositions.Count == 0)
                {
                    throw CreateEmberException(
                        "Unexpected excess End-of-contents identifier at position {0}.", idPosition);
                }

                var endPosition = this.endPositions.Pop();

                if (endPosition.EndPosition.HasValue)
                {
                    throw CreateEmberException(
                        "Unexpected End-of-contents identifier at position {0} for definite length at position {1}.",
                        idPosition,
                        endPosition.LengthPosition);
                }

                return IsContainer(endPosition);
            }
            else
            {
                this.ReadAndProcessLength(id, false);
                return true;
            }
        }

        private void ReadAndProcessInner()
        {
            var innerIdentifierPosition = this.readBuffer.Position;
            var innerIdentifier = ReadIdentifier(this.readBuffer);

            if (innerIdentifier == EndContainer)
            {
                throw CreateEmberException(
                    "Unexpected End-of-contents identifier at position {0}.", innerIdentifierPosition);
            }

            this.ReadAndProcessLength(innerIdentifier, true);
            this.innerNumber = innerIdentifier.ToInnerNumber();

            if (!this.innerNumber.HasValue)
            {
                throw CreateEmberException(
                    "Unexpected context-specific or private identifier at position {0}.", innerIdentifierPosition);
            }

            if (this.innerNumber.Value < Ember.InnerNumber.FirstApplication)
            {
                switch (this.innerNumber.Value)
                {
                    case Ember.InnerNumber.Boolean:
                    case Ember.InnerNumber.Integer:
                    case Ember.InnerNumber.Real:
                    case Ember.InnerNumber.Utf8String:
                    case Ember.InnerNumber.RelativeObjectIdentifier:
                        this.readBuffer.Fill(
                            this.ValidateIdentifierAndLength(innerIdentifier, innerIdentifierPosition));
                        break;
                    case Ember.InnerNumber.Octetstring:
                        this.ValidateIdentifierAndLength(innerIdentifier, innerIdentifierPosition);
                        break;
                    case Ember.InnerNumber.Sequence:
                    case Ember.InnerNumber.Set:
                        break;
                    default:
                        throw CreateEmberException(
                            "Unexpected number in universal identifier at position {0}.", innerIdentifierPosition);
                }
            }
        }

        private void ReadAndProcessLength(EmberId id, bool isInner)
        {
            var lengthPosition = this.readBuffer.Position;
            var length = ReadLength(this.readBuffer);
            this.endPositions.Push(new PositionInfo(id, isInner, lengthPosition, this.readBuffer.Position + length));
        }

        private int ValidateIdentifierAndLength(EmberId innerIdentifier, long innerIdentifierPosition)
        {
            if (innerIdentifier.IsConstructed)
            {
                throw CreateEmberException(
                    "Unexpected constructed encoding at position {0}.", innerIdentifierPosition);
            }

            var length = this.ContentsLength;

            if (!length.HasValue)
            {
                throw CreateEmberException(
                    "Unexpected indefinite length for primitive data value at position {0}.",
                    innerIdentifierPosition);
            }

            this.CanReadContents = true;
            return length.Value;
        }

        private static bool IsContainer(PositionInfo position)
        {
            var emberId = position.EmberId;
            return position.IsInner &&
                ((emberId == Sequence) || (emberId == Set) || (emberId.Class == Class.Application));
        }

        /// <summary>See <i>"X.690"</i><cite>X.690</cite>, chapter 8.1.2.</summary>
        private static EmberId ReadIdentifier(ReadBuffer readBuffer)
        {
            readBuffer.Fill(1);
            var leadingOctet = readBuffer[readBuffer.Index++];
            var theClass = (Class)(leadingOctet & 0xC0);
            var isConstructed = (leadingOctet & 0x20) > 0;
            var number = leadingOctet & 0x1F;
            return new EmberId(theClass, isConstructed, (number <= 30) ? number : Read7Bit(readBuffer));
        }

        /// <summary>See <i>"X.690"</i><cite>X.690</cite>, chapter 8.5 and
        /// <see href="http://technet.microsoft.com/en-us/library/0b34tf65.aspx">IEEE Floating-Point
        /// Representation</see>. Of course the assumption is that C# has the same floating point representation as C++
        /// (pretty safe, as floating point calculations are done by the hardware).</summary>
        private static double ReadReal(ReadBuffer readBuffer, int length)
        {
            if (!BitConverter.IsLittleEndian)
            {
                throw new NotSupportedException("Method is not supported for big endian system.");
            }

            var position = readBuffer.Position;

            // 8.5.2
            if (length == 0)
            {
                return 0.0;
            }

            var firstContentsOctet = readBuffer[readBuffer.Index++];
            --length;
            long sign;
            int exponentLength;

            // 8.5.3 - 8.5.6, encoding must be base 2, so the bits 6 to 3 must be 0. Moreover, bits 8 to 7 must not
            // both be 0 (which would imply a decimal encoding). Finally, bit 2 cannot be 1 if bit 8 is 0 and bit 7 is
            // 1. This leaves exactly the 10 cases enumerated below.
            switch (firstContentsOctet)
            {
                case 0x40:
                    return double.PositiveInfinity; // 8.5.8
                case 0x41:
                    return double.NegativeInfinity; // 8.5.8

                // 8.5.6.4 a)
                case 0x80:
                    sign = 0;
                    exponentLength = 1;
                    break;
                case 0xC0:
                    sign = long.MinValue;
                    exponentLength = 1;
                    break;

                // 8.5.6.4 b)
                case 0x81:
                    sign = 0;
                    exponentLength = 2;
                    break;
                case 0xC1:
                    sign = long.MinValue;
                    exponentLength = 2;
                    break;

                // 8.5.6.4 c)
                case 0x82:
                    sign = 0;
                    exponentLength = 3;
                    break;
                case 0xC2:
                    sign = long.MinValue;
                    exponentLength = 3;
                    break;

                // 8.5.6.4 d)
                case 0x83:
                    sign = 0;
                    exponentLength = readBuffer[readBuffer.Index++];
                    --length;
                    break;
                case 0xC3:
                    sign = long.MinValue;
                    exponentLength = readBuffer[readBuffer.Index++];
                    --length;
                    break;

                default:
                    throw CreateEmberException("Unexpected encoding for Real at position {0}.", position);
            }

            var mantissaLength = length - exponentLength; // 8.5.6.5

            if (mantissaLength < 1)
            {
                // The mantissa can never be 0, so there must be at least one byte for the mantissa.
                throw CreateEmberException("Incorrect length for Real at position {0}.", position);
            }

            var exponent = Read8Bit(readBuffer, exponentLength, true);

            if ((exponent < -Constants.DoubleExponentBias) || (exponent > Constants.DoubleExponentBias))
            {
                throw CreateEmberException(
                    "The exponent of the Real at position {0} exceeds the expected range.", position);
            }

            exponent = (exponent + Constants.DoubleExponentBias) << Constants.DoubleMantissaBits;

            var mantissa = Read8Bit(readBuffer, mantissaLength, false);

            if (mantissa == 0)
            {
                throw CreateEmberException("The mantissa of the Real at position {0} is zero.", position);
            }

            // Normalization, required for IEEE floating point representation
            while ((mantissa & (Constants.DoubleExponentMask >> Constants.BitsPerByte)) == 0)
            {
                mantissa <<= Constants.BitsPerByte;
            }

            // In the 64-bit floating point format, the first non-zero binary digit is not stored but only assumed to
            // be bit 53. We therefore shift until we have the 53rd digit == 1 and then mask it out again.
            while ((mantissa & Constants.DoubleExponentMask) == 0)
            {
                mantissa <<= 1;
            }

            mantissa &= Constants.DoubleMantissaMask;
            return BitConverter.Int64BitsToDouble(sign | exponent | mantissa);
        }

        /// <summary>See <i>"X.690"</i><cite>X.690</cite>, chapter 8.1.3.</summary>
        private static int? ReadLength(ReadBuffer readBuffer)
        {
            var position = readBuffer.Position;
            readBuffer.Fill(1);
            var leadingOctet = readBuffer[readBuffer.Index++];

            if ((leadingOctet & 0x80) > 0)
            {
                var length = leadingOctet & 0x7F;

                if (length == 0)
                {
                    return null;
                }

                readBuffer.Fill(length);
                var result = Read8Bit(readBuffer, length, false);

                if (result > int.MaxValue)
                {
                    throw CreateEmberException("The length at position {0} exceeds the expected range.", position);
                }

                return (int)result;
            }
            else
            {
                return leadingOctet;
            }
        }

        private static long Read8Bit(ReadBuffer readBuffer, int length, bool isSigned)
        {
            if (length <= 0)
            {
                throw new EmberException("Unexpected zero length for integer.");
            }

            var position = readBuffer.Position;
            var mostSignificant = readBuffer[readBuffer.Index++];
            long result;
            long leading;

            // - 1 accounts for the fact that we must not overwrite the sign bit by shifting in bits
            const int MostSignificantShift = Constants.BitsPerLong - Constants.BitsPerByte - 1;

            if (isSigned && ((mostSignificant & 0x80) != 0))
            {
                result = (Constants.AllBitsSetLong << Constants.BitsPerByte) | mostSignificant;
                leading = Constants.AllBitsSetLong << MostSignificantShift;
            }
            else
            {
                result = mostSignificant;
                leading = 0x00;
            }

            for (--length; length > 0; --length)
            {
                const long DiscardBitsMask = Constants.AllBitsSetLong << MostSignificantShift;

                if ((result & DiscardBitsMask) != leading)
                {
                    throw CreateEmberException(
                        "The integer, length or exponent at position {0} exceeds the expected range.", position);
                }

                result <<= Constants.BitsPerByte;
                result |= readBuffer[readBuffer.Index++];
            }

            return result;
        }

        private static int Read7Bit(ReadBuffer readBuffer)
        {
            var position = readBuffer.Position;
            readBuffer.Fill(1);
            byte currentByte;
            var result = 0;

            while (((currentByte = readBuffer[readBuffer.Index++]) & 0x80) > 0)
            {
                result |= currentByte & 0x7F;

                // - 1 accounts for the fact that we must not overwrite the sign bit by shifting in bits
                const int DiscardBitsMask =
                    Constants.AllBitsSetInt << (Constants.BitsPerInt - Constants.BitsPerEncodedByte - 1);

                if ((result & DiscardBitsMask) != 0)
                {
                    throw CreateEmberException(
                        "The identifier number or subidentifier at position {0} exceeds the expected range.", position);
                }

                result <<= Constants.BitsPerEncodedByte;
                readBuffer.Fill(1);
            }

            result |= currentByte;
            return result;
        }

        private static EmberException CreateEmberException(string format, params object[] positions)
        {
            return new EmberException(string.Format(CultureInfo.InvariantCulture, format, positions));
        }

        private static EmberException CreateEmberException(EndOfStreamException ex)
        {
            return new EmberException("Unexpected end of stream.", ex);
        }

        private struct PositionInfo
        {
            private readonly EmberId emberId;
            private readonly bool isInner;
            private readonly long lengthPosition;
            private readonly long? endPosition;

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////

            internal PositionInfo(EmberId emberId, bool isInner, long lengthPosition, long? endPosition)
            {
                this.emberId = emberId;
                this.isInner = isInner;
                this.lengthPosition = lengthPosition;
                this.endPosition = endPosition;
            }

            internal EmberId EmberId
            {
                get { return this.emberId; }
            }

            internal bool IsInner
            {
                get { return this.isInner; }
            }

            internal long LengthPosition
            {
                get { return this.lengthPosition; }
            }

            internal long? EndPosition
            {
                get { return this.endPosition; }
            }
        }
    }
}
