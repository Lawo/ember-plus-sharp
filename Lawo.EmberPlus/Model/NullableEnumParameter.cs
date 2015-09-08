////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    using Lawo.EmberPlus.Ember;
    using Lawo.EmberPlus.Glow;

    /// <summary>Represents a nullable enum parameter in the protocol specified in the
    /// <a href="http://ember-plus.googlecode.com/svn/trunk/documentation/Ember+%20Documentation.pdf">Ember+
    /// Specification</a>.</summary>
    /// <typeparam name="TEnum">The enumeration type.</typeparam>
    /// <remarks>
    /// <para><b>Thread Safety</b>: Any public static members of this type are thread safe. Any instance members are not
    /// guaranteed to be thread safe.</para>
    /// </remarks>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance", Justification = "Fewer levels of inheritance would lead to more code duplication.")]
    public sealed class NullableEnumParameter<TEnum> : NullableParameter<NullableEnumParameter<TEnum>, TEnum?>
        where TEnum : struct
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

        private NullableEnumParameter()
        {
            this.impl = new EnumParameterImpl<TEnum>(this);
        }
    }
}
