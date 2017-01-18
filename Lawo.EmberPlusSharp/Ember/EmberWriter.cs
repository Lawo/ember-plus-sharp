////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Ember
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text;

    using IO;

    /// <summary>Represents a writer that provides the means to generate BER-encoded output, as specified in
    /// <i>"X.690"</i><cite>X.690</cite>.</summary>
    /// <remarks>Only the subset defined in the <i>"Ember+ Specification"</i><cite>Ember+ Specification</cite> is
    /// supported.</remarks>
    /// <threadsafety static="true" instance="false"/>
    public sealed class EmberWriter : IDisposable
    {
        /// <summary>Initializes a new instance of the <see cref="EmberWriter"/> class by calling
        /// <see cref="EmberWriter(Stream, int)">EmberWriter(<paramref name="stream"/>, 1024)</see>.</summary>
        public EmberWriter(Stream stream)
            : this(stream, Constants.MemoryStreamBufferSize)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="EmberWriter"/> class.</summary>
        /// <param name="stream">The stream to which the EmBER-encoded output should be written.</param>
        /// <param name="bufferSize">The size of the internal buffer in bytes.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> equals <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="bufferSize"/> is 0 or negative.</exception>
        public EmberWriter(Stream stream, int bufferSize)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            this.writeBuffer = new WriteBuffer(stream.Write, bufferSize);
            this.tempBuffer = new WriteBuffer(this.writeBuffer.Write, SubidentifierMaxLength * 16);
            this.stream = stream;
        }

        /// <summary>Releases all resources used by the current instance of the <see cref="EmberWriter"/> class.</summary>
        /// <remarks>Flushes the internal buffer into the stream passed to the constructor and then calls
        /// <see cref="Stream.Dispose()"/>.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Dispose() must never throw.")]
        public void Dispose()
        {
            try
            {
                if (this.stream != null)
                {
                    this.writeBuffer.Flush();
                    this.stream.Dispose();
                }
            }
            catch
            {
            }
            finally
            {
                this.stream = null;
            }
        }

        /// <summary>Writes <paramref name="outer"/> with definite length followed by <paramref name="value"/> as
        /// Boolean.</summary>
        /// <exception cref="ObjectDisposedException"><see cref="Dispose"/> has been called.</exception>
        public void WriteValue(EmberId outer, bool value)
        {
            this.AssertNotDisposed();
            this.writeBuffer.Reserve(IdentifiersAndLengthsMaxLength + 1);
            this.WriteIdentifiersAndLengths(outer, Boolean, 1);
            Write8Bit(this.writeBuffer, value ? Constants.AllBitsSetLong : 0, 0); // AllBitsSet is encoded as 0xFF
        }

        /// <summary>Writes <paramref name="outer"/> with definite length followed by <paramref name="value"/> as
        /// Integer.</summary>
        /// <exception cref="ObjectDisposedException"><see cref="Dispose"/> has been called.</exception>
        public void WriteValue(EmberId outer, long value)
        {
            this.AssertNotDisposed();
            this.writeBuffer.Reserve(IdentifiersAndLengthsMaxLength + Constants.BytesPerLong);
            var shift = Get8BitStartShift(value, true);
            this.WriteIdentifiersAndLengths(outer, Integer, GetLengthFromShift8Bit(shift));
            Write8Bit(this.writeBuffer, value, shift);
        }

        /// <summary>Writes <paramref name="outer"/> with definite length followed by <paramref name="value"/> as
        /// Octetstring.</summary>
        /// <exception cref="ObjectDisposedException"><see cref="Dispose"/> has been called.</exception>
        public void WriteValue(EmberId outer, byte[] value)
        {
            this.AssertNotDisposed();

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            // For byte[] values, the buffer size does not matter
            this.writeBuffer.Reserve(IdentifiersAndLengthsMaxLength);

            this.WriteIdentifiersAndLengths(outer, Octetstring, value.Length);
            this.writeBuffer.Write(value, 0, value.Length);
        }

        /// <summary>Writes <paramref name="outer"/> with definite length followed by <paramref name="value"/> as
        /// Real.</summary>
        /// <exception cref="NotSupportedException">This method is called on a big endian CPU, which is currently not
        /// supported.</exception>
        /// <exception cref="ObjectDisposedException"><see cref="Dispose"/> has been called.</exception>
        public void WriteValue(EmberId outer, double value)
        {
            this.AssertNotDisposed();

            this.writeBuffer.Reserve(IdentifiersAndLengthsMaxLength);
            this.tempBuffer.Reserve(10); // 1st byte followed by max. 2 exponent bytes, followed by max 7 mantissa bytes

            try
            {
                WriteReal(this.tempBuffer, value);
                this.WriteIdentifiersAndLengths(outer, Real, this.tempBuffer.Count);
                this.tempBuffer.Flush();
            }
            finally
            {
                this.tempBuffer.Count = 0;
            }
        }

        /// <summary>Writes <paramref name="outer"/> with definite length followed by <paramref name="value"/> as
        /// UTF8String.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> equals <c>null</c>.</exception>
        /// <exception cref="ObjectDisposedException"><see cref="Dispose"/> has been called.</exception>
        public void WriteValue(EmberId outer, string value)
        {
            this.AssertNotDisposed();

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var byteCount = Encoding.UTF8.GetByteCount(value);
            this.writeBuffer.Reserve(IdentifiersAndLengthsMaxLength + byteCount);
            this.WriteIdentifiersAndLengths(outer, Utf8String, byteCount);
            this.writeBuffer.WriteAsUtf8(value, byteCount);
        }

        /// <summary>Writes <paramref name="outer"/> with definite length followed by <paramref name="value"/> as
        /// Relative object identifier.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> equals <c>null</c>.</exception>
        /// <exception cref="ObjectDisposedException"><see cref="Dispose"/> has been called.</exception>
        public void WriteValue(EmberId outer, int[] value)
        {
            this.AssertNotDisposed();

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            // For byte[] values, the buffer size does not matter
            this.writeBuffer.Reserve(IdentifiersAndLengthsMaxLength);
            this.tempBuffer.Reserve(SubidentifierMaxLength * value.Length);

            try
            {
                // See http://www.itu.int/ITU-T/studygroups/com17/languages/X.690-0207.pdf, chapter 8.20.2.
                foreach (int number in value)
                {
                    Write7Bit(this.tempBuffer, number, Get7BitStartShift(number));
                }

                this.WriteIdentifiersAndLengths(outer, RelativeObjectIdentifier, this.tempBuffer.Count);
                this.tempBuffer.Flush();
            }
            finally
            {
                // Make sure that tempBuffer is empty if anything goes wrong
                this.tempBuffer.Count = 0;
            }
        }

        /// <summary>Writes <paramref name="outer"/> with indefinite length followed by the start of a sequence with
        /// indefinite length.</summary>
        /// <exception cref="ObjectDisposedException"><see cref="Dispose"/> has been called.</exception>
        public void WriteStartSequence(EmberId outer)
        {
            this.AssertNotDisposed();
            this.writeBuffer.Reserve(IdentifiersAndLengthsMaxLength);
            this.WriteIdentifiersAndLengths(outer, Sequence, null);
        }

        /// <summary>Writes <paramref name="outer"/> with indefinite length followed by the start of a set with
        /// indefinite length.</summary>
        /// <exception cref="ObjectDisposedException"><see cref="Dispose"/> has been called.</exception>
        public void WriteStartSet(EmberId outer)
        {
            this.AssertNotDisposed();
            this.writeBuffer.Reserve(IdentifiersAndLengthsMaxLength);
            this.WriteIdentifiersAndLengths(outer, Set, null);
        }

        /// <summary>Writes <paramref name="outer"/> with indefinite length followed by the start of an
        /// application-defined type with indefinite length.</summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="innerNumber"/> is smaller than
        /// <see cref="InnerNumber.FirstApplication"/>.</exception>
        /// <exception cref="ObjectDisposedException"><see cref="Dispose"/> has been called.</exception>
        public void WriteStartApplicationDefinedType(EmberId outer, int innerNumber)
        {
            this.AssertNotDisposed();

            if (innerNumber < InnerNumber.FirstApplication)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(innerNumber), "Must be greater than or equal to InnerNumber.StartFirstApplication");
            }

            this.writeBuffer.Reserve(IdentifiersAndLengthsMaxLength);
            this.WriteIdentifiersAndLengths(outer, EmberId.FromInnerNumber(innerNumber), null);
        }

        /// <summary>Writes the end of the previous sequence, set or application-defined type.</summary>
        /// <exception cref="ObjectDisposedException"><see cref="Dispose"/> has been called.</exception>
        public void WriteEndContainer()
        {
            this.AssertNotDisposed();
            this.writeBuffer.Reserve(4);
            WriteIdentifier(this.writeBuffer, EndContainer);
            WriteLength(this.writeBuffer, 0, 0, 1);
            WriteIdentifier(this.writeBuffer, EndContainer);
            WriteLength(this.writeBuffer, 0, 0, 1);
        }

        /// <summary>Flushes the internal buffer into the stream passed to the constructor.</summary>
        public void Flush()
        {
            this.AssertNotDisposed();
            this.writeBuffer.Flush();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private const int StartShift8Bit = Constants.BitsPerLong - Constants.BitsPerByte;
        private const int StartShift7Bit =
            Constants.BitsPerInt / Constants.BitsPerEncodedByte * Constants.BitsPerEncodedByte;

        private const int LengthMaxLength = (StartShift8Bit / Constants.BitsPerByte) + 1 + 1;
        private const int SubidentifierMaxLength = (Constants.BitsPerInt / Constants.BitsPerEncodedByte) + 1;
        private const int IdentifierMaxLength = SubidentifierMaxLength + 1;
        private const int IdentifiersAndLengthsMaxLength = (2 * IdentifierMaxLength) + (2 * LengthMaxLength);

        private static readonly EmberId EndContainer = EmberId.CreateUniversal(InnerNumber.EndContainer);
        private static readonly EmberId Boolean = EmberId.CreateUniversal(InnerNumber.Boolean);
        private static readonly EmberId Integer = EmberId.CreateUniversal(InnerNumber.Integer);
        private static readonly EmberId Octetstring = EmberId.CreateUniversal(InnerNumber.Octetstring);
        private static readonly EmberId Real = EmberId.CreateUniversal(InnerNumber.Real);
        private static readonly EmberId Utf8String = EmberId.CreateUniversal(InnerNumber.Utf8String);
        private static readonly EmberId RelativeObjectIdentifier =
            EmberId.CreateUniversal(InnerNumber.RelativeObjectIdentifier);

        private static readonly EmberId Sequence = EmberId.CreateUniversal(InnerNumber.Sequence);
        private static readonly EmberId Set = EmberId.CreateUniversal(InnerNumber.Set);
        private static readonly long NegativeZeroInteger = BitConverter.DoubleToInt64Bits(-0.0);

        private static int Get8BitStartShift(long value, bool isSigned)
        {
            if ((value >= sbyte.MinValue) && (value <= sbyte.MaxValue))
            {
                return 0;
            }

            var leading = value < 0 ? 0xFFL : 0x00L;
            int shift;
            long currentByte;

            for (shift = StartShift8Bit;
                ((currentByte = (value >> shift) & 0xFF) == leading) && (shift > 0); shift -= Constants.BitsPerByte)
            {
            }

            if (isSigned && ((value > 0) == ((currentByte & 0x80) != 0)))
            {
                shift += Constants.BitsPerByte;
            }

            return shift;
        }

        private static int Get7BitStartShift(int value)
        {
            int shift;

            for (shift = StartShift7Bit; (shift > 0) && (((Constants.AllBitsSetInt << shift) & value) == 0);
                shift -= Constants.BitsPerEncodedByte)
            {
            }

            return shift;
        }

        /// <summary>See <i>"X.690"</i><cite>X.690</cite>, chapter 8.1.2.</summary>
        private static void WriteIdentifier(WriteBuffer writeBuffer, EmberId emberId)
        {
            if (emberId.Number <= 30)
            {
                writeBuffer[writeBuffer.Count++] = GetLeadingOctet(emberId, emberId.Number);
            }
            else
            {
                writeBuffer[writeBuffer.Count++] = GetLeadingOctet(emberId, 0x1F);
                Write7Bit(writeBuffer, emberId.Number, Get7BitStartShift(emberId.Number));
            }
        }

        /// <summary>See <i>"X.690"</i><cite>X.690</cite>, chapter 8.1.3.</summary>
        private static void WriteLength(WriteBuffer writeBuffer, int? length, int shift, int lengthLength)
        {
            if (lengthLength == 1)
            {
                writeBuffer[writeBuffer.Count++] = length.HasValue ? (byte)length : (byte)0x80;
            }
            else
            {
                writeBuffer[writeBuffer.Count++] = (byte)((lengthLength - 1) | 0x80);
                Write8Bit(writeBuffer, length.GetValueOrDefault(), shift);
            }
        }

        /// <summary>See <i>"X.690"</i><cite>X.690</cite>, chapter 8.5 and
        /// <see href="http://technet.microsoft.com/en-us/library/0b34tf65.aspx">IEEE Floating-Point
        /// Representation</see>. Of course the assumption is that C# has the same floating point representation as C++
        /// (pretty safe, as floating point calculations are done by the hardware).</summary>
        private static void WriteReal(WriteBuffer writeBuffer, double value)
        {
            if (!BitConverter.IsLittleEndian)
            {
                throw new NotSupportedException("Method is not supported for big endian system.");
            }

            if (double.IsInfinity(value))
            {
                writeBuffer[writeBuffer.Count++] = (byte)(value > 0 ? 0x40 : 0x41); // 8.5.6 c) and 8.5.9
                return;
            }

            if (double.IsNaN(value))
            {
                writeBuffer[writeBuffer.Count++] = 0x42; // 8.5.9
                return;
            }

            var bits = BitConverter.DoubleToInt64Bits(value);

            if (bits == NegativeZeroInteger)
            {
                writeBuffer[writeBuffer.Count++] = 0x43; // 8.5.3 and 8.5.9
                return;
            }

            // 8.5.2
            if (bits == 0)
            {
                return;
            }

            // 8.5.6 a)
            byte firstContentsOctet = 0x80;

            const long SignMask = long.MinValue;

            // 8.5.7.1
            if ((bits & SignMask) != 0)
            {
                firstContentsOctet |= 0x40;
            }

            var exponent =
                ((bits & Constants.DoubleExponentMask) >> Constants.DoubleMantissaBits) - Constants.DoubleExponentBias;
            var exponentShift = Get8BitStartShift(exponent, true);
            firstContentsOctet |= (byte)(GetLengthFromShift8Bit(exponentShift) - 1); // 8.5.7.4
            writeBuffer[writeBuffer.Count++] = firstContentsOctet;
            Write8Bit(writeBuffer, exponent, exponentShift);

            const long MantissaAssumedLeadingOne = 1L << Constants.DoubleMantissaBits;

            var mantissa = (bits & Constants.DoubleMantissaMask) | MantissaAssumedLeadingOne;

            // CER denormalization 11.3.1 (not required but saves space)
            while ((mantissa & 0xFF) == 0)
            {
                mantissa >>= Constants.BitsPerByte;
            }

            while ((mantissa & 0x01) == 0)
            {
                mantissa >>= 1;
            }

            // TODO: According to 8.5.7.5 we should pass false below, but we pass true to avoid a bug in EmberLib.
            Write8Bit(writeBuffer, mantissa, Get8BitStartShift(mantissa, true)); // 8.5.6.5
        }

        private static void Write8Bit(WriteBuffer writeBuffer, long value, int shift)
        {
            for (; shift >= 0; shift -= 8)
            {
                writeBuffer[writeBuffer.Count++] = (byte)((value >> shift) & 0xFF);
            }
        }

        private static void Write7Bit(WriteBuffer writeBuffer, long value, int shift)
        {
            for (; shift > 0; shift -= 7)
            {
                writeBuffer[writeBuffer.Count++] = (byte)(((value >> shift) & 0x7F) | 0x80);
            }

            writeBuffer[writeBuffer.Count++] = (byte)(value & 0x7F);
        }

        private static byte GetLeadingOctet(EmberId emberId, int bits)
        {
            const int PrimitiveFlag = 0x00;
            const int ConstructedFlag = 0x20;

            return (byte)((int)emberId.Class | (emberId.IsConstructed ? ConstructedFlag : PrimitiveFlag) | bits);
        }

        private static int GetLengthFromShift8Bit(int shift) => (shift / Constants.BitsPerByte) + 1;

        private static int GetLengthLength(int? length, out int shift)
        {
            if (!length.HasValue || (length <= sbyte.MaxValue))
            {
                shift = 0;
                return 1;
            }
            else if (length <= byte.MaxValue)
            {
                // Since lengths are unsigned we will get a shift of 0 for lengths < 256. A shift of 0 is equivalent to 1
                // byte, which is wrong for lengths > 127, see 8.1.3.5. For a shift > 0, the long form will be employed in
                // any case, so the correction below really only applies when value is in the range [128, 255].
                shift = 0;
                return 2;
            }
            else
            {
                shift = Get8BitStartShift(length.Value, false);
                return GetLengthFromShift8Bit(shift) + 1;
            }
        }

        private readonly WriteBuffer writeBuffer;
        private readonly WriteBuffer tempBuffer;
        private Stream stream;

        private void AssertNotDisposed()
        {
            if (this.stream == null)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }

        private void WriteIdentifiersAndLengths(EmberId outer, EmberId inner, int? innerLength)
        {
            // The outer length is the inner length + the length of the inner length field + the length of the inner
            // token (for definite lengths the inner token is always universal and therefore one byte).
            int innerShift;
            var innerLengthLength = GetLengthLength(innerLength, out innerShift);
            var outerLength = innerLength + innerLengthLength + 1;
            int outerShift;
            var outerLengthLength = GetLengthLength(outerLength, out outerShift);
            WriteIdentifier(this.writeBuffer, outer);
            WriteLength(this.writeBuffer, outerLength, outerShift, outerLengthLength);
            WriteIdentifier(this.writeBuffer, inner);
            WriteLength(this.writeBuffer, innerLength, innerShift, innerLengthLength);
        }
    }
}
