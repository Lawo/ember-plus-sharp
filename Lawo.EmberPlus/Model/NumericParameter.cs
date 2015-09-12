////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>Represents a numeric parameter in the object tree accessible through
    /// <see cref="Consumer{T}.Root">Consumer&lt;TRoot&gt;.Root</see>.</summary>
    /// <typeparam name="TMostDerived">The most-derived subtype of this class.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <threadsafety static="true" instance="false"/>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance", Justification = "Fewer levels of inheritance would lead to more code duplication.")]
    public abstract class NumericParameter<TMostDerived, TValue> : ValueParameter<TMostDerived, TValue>
        where TMostDerived : NumericParameter<TMostDerived, TValue>
        where TValue : struct
    {
        private TValue? minimum;
        private TValue? maximum;
        private string formula;

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

        /// <summary>Gets formula, see <see cref="IParameter.Formula"/>.</summary>
        public string Formula
        {
            get { return this.formula; }
            private set { this.SetValue(ref this.formula, value); }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal NumericParameter()
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

        internal sealed override string FormulaCore
        {
            get { return this.Formula; }
            set { this.Formula = value; }
        }
    }
}
