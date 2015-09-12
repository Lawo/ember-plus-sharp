////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    using Ember;
    using Glow;

    /// <summary>Represents an integer parameter in the object tree accessible through
    /// <see cref="Consumer{T}.Root">Consumer&lt;TRoot&gt;.Root</see>.</summary>
    /// <threadsafety static="true" instance="false"/>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance", Justification = "Fewer levels of inheritance would lead to more code duplication.")]
    public sealed class IntegerParameter : NumericParameter<IntegerParameter, long>
    {
        private int? factor;
        private IReadOnlyList<KeyValuePair<string, int>> enumMap;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <inheritdoc cref="IParameter.Factor"/>
        public int? Factor
        {
            get { return this.factor; }
            private set { this.SetValue(ref this.factor, value); }
        }

        /// <inheritdoc cref="IParameter.EnumMap"/>
        public IReadOnlyList<KeyValuePair<string, int>> EnumMap
        {
            get { return this.enumMap; }
            private set { this.SetValue(ref this.enumMap, value); }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal sealed override int? FactorCore
        {
            get { return this.Factor; }
            set { this.Factor = value; }
        }

        internal sealed override IReadOnlyList<KeyValuePair<string, int>> EnumMapCore
        {
            get { return this.EnumMap; }
            set { this.EnumMap = value; }
        }

        internal sealed override long? ReadValue(EmberReader reader, out ParameterType? parameterType)
        {
            parameterType = ParameterType.Integer;
            return reader.AssertAndReadContentsAsInt64();
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Method is not public, CA bug?")]
        internal sealed override void WriteValue(EmberWriter writer, long? value)
        {
            writer.WriteValue(GlowParameterContents.Value.OuterId, value.Value);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private IntegerParameter()
        {
        }
    }
}
