////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>This class is not intended to be referenced in your code.</summary>
    /// <remarks>Provides the common implementation for all parameters with a reference type value in the object tree
    /// accessible through <see cref="Consumer{T}.Root">Consumer&lt;TRoot&gt;.Root</see>.</remarks>
    /// <typeparam name="TMostDerived">The most-derived subtype of this class.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <threadsafety static="true" instance="false"/>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance", Justification = "Fewer levels of inheritance would lead to more code duplication.")]
    public abstract class ReferenceParameter<TMostDerived, TValue> : Parameter<TMostDerived, TValue>
        where TMostDerived : ReferenceParameter<TMostDerived, TValue>
    {
        /// <summary>Gets or sets value.</summary>
        /// <value>The last value sent by the provider or set by calling code. Cannot be <c>null</c>. The provider is
        /// expected to send an initial value != null. Failure to do so will result in a <see cref="ModelException"/>
        /// being thrown from <see cref="Consumer{T}.CreateAsync(S101.S101Client)"/>.</value>
        /// <exception cref="ArgumentNullException">Attempted to set to <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Attempted to access the value in a way that conflicts with
        /// <see cref="ParameterBase{T, U}.Access"/>.</exception>
        public TValue Value
        {
            get { return this.ValueCore; }
            set { this.ValueCore = value; }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal ReferenceParameter()
        {
        }
    }
}
