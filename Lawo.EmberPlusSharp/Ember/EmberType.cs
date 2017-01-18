////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Ember
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>Describes an EmBER type.</summary>
    /// <remarks>
    /// <para>An <see cref="EmberType"/> instance contains most of the information that is necessary to read and write
    /// a data value of a given type with <see cref="EmberReader"/> and <see cref="EmberWriter"/>. Moreover, it also
    /// contains the information to convert a data value to and from XML.</para>
    /// <para>An <see cref="EmberType"/> object is typically created by providing a <see cref="System.Type"/> object
    /// representing a class that contains two constants describing the type itself and that optionally contains one
    /// nested class describing each field. Each of the nested classes in turn contains 3 constants. So, a primitive
    /// type (which does not contain any fields) can be represented like e.g. <see cref="BerBoolean"/>,
    /// <see cref="BerReal"/>, etc. A type with fields, like e.g. the Glow Command, can be described as follows:
    /// </para>
    /// <code>
    /// internal static class GlowCommand
    /// {
    ///     internal const int InnerNumber = Ember.InnerNumber.FirstApplication + 2;
    ///     internal const string Name = "Command";
    ///
    ///     internal static class Number
    ///     {
    ///         internal const int OuterNumber = 0;
    ///         internal const string Name = "number";
    ///         internal static readonly EmberId OuterId = EmberId.CreateContextSpecific(OuterNumber);
    ///     }
    ///
    ///     internal static class DirFieldMask
    ///     {
    ///         internal const int OuterNumber = 1;
    ///         internal const string Name = "dirFieldMask";
    ///         internal static readonly EmberId OuterId = EmberId.CreateContextSpecific(OuterNumber);
    ///     }
    /// }
    /// </code>
    /// <para>This form of description was chosen so that e.g. code that turns a serialized Glow Command into an
    /// in-memory representation can directly use these constants. On the other hand, code that needs to transform a
    /// serialized EmBER representation to and from e.g. XML, can easily use reflection to build high performance lookup
    /// tables from the information contained in these classes.</para>
    /// <para>The fields of a type can usually unambiguously be identified by an <see cref="EmberType"/> instance
    /// constructed by passing a single <see cref="System.Type"/> instance. However, certain DTDs store fields in plain BER
    /// sequence or set types. For these fields, it is necessary to also consider one or more parent fields. This is why
    /// the constructor also accepts two or more types, where the first types represent the path to the last type.
    /// </para>
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    public sealed class EmberType
    {
        /// <summary>Implicitly converts <paramref name="type"/> to a <see cref="EmberType"/>.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> equals <c>null</c>.</exception>
        public static implicit operator EmberType(Type type) => FromType(type);

        /// <summary>Creates a <see cref="EmberType"/> from <paramref name="type"/>.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> equals <c>null</c>.</exception>
        public static EmberType FromType(Type type) => new EmberType(type);

        /// <summary>Initializes a new instance of the <see cref="EmberType"/> class.</summary>
        /// <param name="types">The parent fields followed by the actual type, in descending order.</param>
        /// <exception cref="ArgumentNullException"><paramref name="types"/> equals <c>null</c>.</exception>
        public EmberType(params Type[] types)
        {
            if (types == null)
            {
                throw new ArgumentNullException(nameof(types));
            }

            if (types.Length == 0)
            {
                throw new ArgumentException("Length must not be 0.", nameof(types));
            }

            this.types = types;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal IEnumerable<Type> OuterFields => this.types.Take(this.types.Length - 1);

        internal Type Type => this.types[this.types.Length - 1];

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private readonly Type[] types;
    }
}
