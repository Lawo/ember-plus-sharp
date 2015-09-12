////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>Represents a nullable numeric parameter in the object tree accessible through
    /// <see cref="Consumer{T}.Root">Consumer&lt;TRoot&gt;.Root</see>.</summary>
    /// <typeparam name="TMostDerived">The most-derived subtype of this class.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <threadsafety static="true" instance="false"/>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance", Justification = "Fewer levels of inheritance would lead to more code duplication.")]
    public abstract class NullableNumericParameter<TMostDerived, TValue> : NullableParameter<TMostDerived, TValue?>
        where TMostDerived : NullableNumericParameter<TMostDerived, TValue>
        where TValue : struct
    {
        private TValue? minimum;
        private TValue? maximum;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

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

        internal sealed override object GetMinimum()
        {
            return this.Minimum;
        }

        internal sealed override void SetMinimum(TValue? value)
        {
            this.Minimum = value;
        }

        internal sealed override object GetMaximum()
        {
            return this.Maximum;
        }

        internal sealed override void SetMaximum(TValue? value)
        {
            this.Maximum = value;
        }
    }
}
