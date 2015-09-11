////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    using Ember;
    using Glow;

    /// <summary>Represents an enum parameter in the protocol specified in the
    /// <a href="http://ember-plus.googlecode.com/svn/trunk/documentation/Ember+%20Documentation.pdf">Ember+
    /// Specification</a>.</summary>
    /// <typeparam name="TEnum">The enumeration type.</typeparam>
    /// <threadsafety static="true" instance="false"/>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance", Justification = "Fewer levels of inheritance would lead to more code duplication.")]
    public sealed class EnumParameter<TEnum> : ValueParameter<EnumParameter<TEnum>, TEnum> where TEnum : struct
    {
        private readonly EnumParameterImpl<TEnum> impl;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Method is not public, CA bug?")]
        internal sealed override IReadOnlyList<KeyValuePair<string, int>> EnumMapCore
        {
            get { return this.impl.EnumMapCore; }
            set { this.impl.EnumMapCore = value; }
        }

        internal sealed override ChildrenState ReadContents(EmberReader reader, ElementType actualType)
        {
            return this.impl.ReadContents(base.ReadContents, reader, actualType);
        }

        internal sealed override TEnum? ReadValue(EmberReader reader, out ParameterType? parameterType)
        {
            parameterType = ParameterType.Enum;
            return FastEnum.ToEnum<TEnum>(reader.AssertAndReadContentsAsInt64());
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Method is not public, CA bug?")]
        internal sealed override void WriteValue(EmberWriter writer, TEnum? value)
        {
            writer.WriteValue(GlowParameterContents.Value.OuterId, FastEnum.ToInt64(value.Value));
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private EnumParameter()
        {
            this.impl = new EnumParameterImpl<TEnum>(this);
        }
    }
}
