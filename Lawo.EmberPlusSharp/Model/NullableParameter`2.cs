////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System;

    /// <summary>This class is not intended to be referenced in your code.</summary>
    /// <remarks>Provides the common implementation for all nullable parameters in the object tree accessible through
    /// <see cref="Consumer{T}.Root">Consumer&lt;TRoot&gt;.Root</see>.</remarks>
    /// <typeparam name="TMostDerived">The most-derived subtype of this class.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <threadsafety static="true" instance="false"/>
    public abstract class NullableParameter<TMostDerived, TValue> : ParameterBase<TMostDerived, TValue>
        where TMostDerived : NullableParameter<TMostDerived, TValue>
    {
        /// <summary>Gets or sets <b>value</b>.</summary>
        /// <value>The last value sent by the provider or set by calling code. Can be <c>null</c> if the provider did
        /// not send an initial value and calling code has not set one.</value>
        /// <exception cref="ArgumentNullException">Attempted to set to <c>null</c>.</exception>
        /// <exception cref="ModelException">Attempted to access the value in a way that conflicts with
        /// <see cref="ParameterBase{T, U}.Access"/>.</exception>
        public TValue Value
        {
            get { return this.ValueCore; }
            set { this.ValueCore = value; }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal NullableParameter()
        {
        }
    }
}
