////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>This class is not intended to be referenced in your code.</summary>
    /// <remarks>Provides the common implementation for all nullable numeric parameters in the object tree accessible
    /// through <see cref="Consumer{T}.Root">Consumer&lt;TRoot&gt;.Root</see>.</remarks>
    /// <typeparam name="TMostDerived">The most-derived subtype of this class.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <threadsafety static="true" instance="false"/>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance", Justification = "Fewer levels of inheritance would lead to more code duplication.")]
    public abstract class NullableNumericParameter<TMostDerived, TValue> : NullableParameter<TMostDerived, TValue?>
        where TMostDerived : NullableNumericParameter<TMostDerived, TValue>
        where TValue : struct
    {
        /// <summary>Gets minimum.</summary>
        public TValue? Minimum
        {
            get { return this.minimum; }
            private set { this.SetValue(ref this.minimum, value); }
        }

        /// <summary>Gets maximum.</summary>
        public TValue? Maximum
        {
            get { return this.maximum; }
            private set { this.SetValue(ref this.maximum, value); }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal NullableNumericParameter()
        {
        }

        internal sealed override object GetMinimum() => this.Minimum;

        internal sealed override void SetMinimum(TValue? value) => this.Minimum = value;

        internal sealed override object GetMaximum() => this.Maximum;

        internal sealed override void SetMaximum(TValue? value) => this.Maximum = value;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private TValue? minimum;
        private TValue? maximum;
    }
}
