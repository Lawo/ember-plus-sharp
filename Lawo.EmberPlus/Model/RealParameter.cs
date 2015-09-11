﻿////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model
{
    using System.Diagnostics.CodeAnalysis;

    using Ember;
    using Glow;

    /// <summary>Represents a real parameter in the protocol specified in the
    /// <a href="http://ember-plus.googlecode.com/svn/trunk/documentation/Ember+%20Documentation.pdf">Ember+
    /// Specification</a>.</summary>
    /// <threadsafety static="true" instance="false"/>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance", Justification = "Fewer levels of inheritance would lead to more code duplication.")]
    public sealed class RealParameter : NumericParameter<RealParameter, double>
    {
        internal sealed override double? ReadValue(EmberReader reader, out ParameterType? parameterType)
        {
            parameterType = ParameterType.Real;
            return reader.AssertAndReadContentsAsDouble();
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Method is not public, CA bug?")]
        internal sealed override void WriteValue(EmberWriter writer, double? value)
        {
            writer.WriteValue(GlowParameterContents.Value.OuterId, value.Value);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private RealParameter()
        {
        }
    }
}
