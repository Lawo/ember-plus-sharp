////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2016 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Ember
{
    using System;
    using System.Globalization;

    /// <summary>Represents a BER identifier as specified in <i>"X.690"</i><cite>X.690</cite>, chapter 8.1.2.</summary>
    /// <remarks>Only the subset defined in the <i>"Ember+ Specification"</i><cite>Ember+ Specification</cite> is
    /// supported.</remarks>
    /// <threadsafety static="true" instance="false"/>
    public struct EmberId : IEquatable<EmberId>
    {
        /// <summary>Tests whether two <see cref="EmberId"/> structures are equal.</summary>
        public static bool operator ==(EmberId left, EmberId right)
        {
            return left.Equals(right);
        }

        /// <summary>Tests whether two <see cref="EmberId"/> structures differ.</summary>
        public static bool operator !=(EmberId left, EmberId right)
        {
            return !left.Equals(right);
        }

        /// <summary>Creates a constructed identifier of the Application class with the specified number.</summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="number"/> is negative.</exception>
        public static EmberId CreateApplication(int number)
        {
            return new EmberId(Class.Application, true, number);
        }

        /// <summary>Creates a constructed identifier of the Context-specific class with the specified number.</summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="number"/> is negative.</exception>
        public static EmberId CreateContextSpecific(int number)
        {
            return new EmberId(Class.ContextSpecific, true, number);
        }

        /// <summary>Converts the string representation of an identifier <paramref name="input"/> into its
        /// <see cref="EmberId"/> equivalent and returns a value whether the conversion succeeded.</summary>
        /// <returns><c>true</c> if <paramref name="input"/> was converted successfully; otherwise <c>false</c>.</returns>
        public static bool TryParse(string input, out EmberId emberId)
        {
            Class? theClass;
            int number;

            if ((input != null) && (input.Length >= 3) && ((theClass = FromChar(input[0])) != null) &&
                (input[1] == '-') &&
                int.TryParse(input.Substring(2), NumberStyles.None, CultureInfo.InvariantCulture, out number))
            {
                var constructed = (theClass.Value != Class.Universal) ||
                    (number == InnerNumber.Sequence) || (number == InnerNumber.Set);
                emberId = new EmberId(theClass.Value, constructed, number);
                return true;
            }
            else
            {
                emberId = default(EmberId);
                return false;
            }
        }

        /// <inheritdoc/>
        public bool Equals(EmberId other)
        {
            return (this.theClass == other.theClass) &&
                (this.isConstructed == other.isConstructed) && (this.number == other.number);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            var other = obj as EmberId?;
            return other.HasValue && this.Equals(other.Value);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine((int)this.theClass, this.isConstructed ? 1 : 0, this.number);
        }

        /// <summary>Returns a string that represents the current object.</summary>
        public override string ToString()
        {
            return ToChar(this.theClass) + "-" + this.number.ToString(CultureInfo.InvariantCulture);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal static EmberId CreateUniversal(int number)
        {
            return new EmberId(
                Class.Universal, (number == InnerNumber.Sequence) || (number == InnerNumber.Set), number);
        }

        internal static EmberId FromInnerNumber(int innerNumber)
        {
            return innerNumber < InnerNumber.FirstApplication ?
                CreateUniversal(innerNumber) : CreateApplication(innerNumber - InnerNumber.FirstApplication);
        }

        internal EmberId(Class theClass, bool isConstructed, int number)
        {
            if (number < 0)
            {
                throw new ArgumentOutOfRangeException("number", ExceptionMessages.NonnegativeNumberRequired);
            }

            this.theClass = theClass;
            this.isConstructed = isConstructed;
            this.number = number;
        }

        internal Class Class
        {
            get { return this.theClass; }
        }

        internal bool IsConstructed
        {
            get { return this.isConstructed; }
        }

        internal int Number
        {
            get { return this.number; }
        }

        internal int? ToInnerNumber()
        {
            switch (this.theClass)
            {
                case Class.Universal:
                    return this.number;
                case Class.Application:
                    return this.number + InnerNumber.FirstApplication;
                default:
                    return null;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static char ToChar(Class theClass)
        {
            switch (theClass)
            {
                case Class.Universal:
                    return 'U';
                case Class.Application:
                    return 'A';
                case Class.ContextSpecific:
                    return 'C';
                default:
                    return 'P';
            }
        }

        private static Class? FromChar(char c)
        {
            switch (c)
            {
                case 'U':
                    return Class.Universal;
                case 'A':
                    return Class.Application;
                case 'C':
                    return Class.ContextSpecific;
                case 'P':
                    return Class.Private;
                default:
                    return null;
            }
        }

        private readonly Class theClass;
        private readonly bool isConstructed;
        private readonly int number;
    }
}
