////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
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

        internal sealed override void WriteValue(EmberWriter writer, long? value) =>
            writer.WriteValue(GlowParameterContents.Value.OuterId, value.GetValueOrDefault());

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private int? factor;
        private IReadOnlyList<KeyValuePair<string, int>> enumMap;

        private IntegerParameter()
        {
        }
    }
}
