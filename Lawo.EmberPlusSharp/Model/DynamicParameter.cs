////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    using Ember;
    using Glow;

    /// <summary>Represents a parameter for which the value type is determined at runtime.</summary>
    /// <remarks>The annoying number of switch-case statements stems from the fact that a provider can send the fields
    /// of a parameter in any order, such that the type only becomes clear when the very last field is read.</remarks>
    internal sealed class DynamicParameter : ParameterBase<DynamicParameter, object>
    {
        internal DynamicParameter()
        {
        }

        internal sealed override int? FactorCore
        {
            get { return this.factor; }
            set { this.SetValue(ref this.factor, value, "Factor"); }
        }

        internal sealed override string FormulaCore
        {
            get { return this.formula; }
            set { this.SetValue(ref this.formula, value, "Formula"); }
        }

        internal sealed override IReadOnlyList<KeyValuePair<string, int>> EnumMapCore
        {
            get { return this.enumMap; }
            set { this.SetValue(ref this.enumMap, value, "EnumMap"); }
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Method is not public, CA bug?")]
        internal sealed override object ReadValue(EmberReader reader, out ParameterType? parameterType)
        {
            switch (reader.InnerNumber)
            {
                case InnerNumber.Boolean:
                    parameterType = ParameterType.Boolean;
                    return reader.ReadContentsAsBoolean();
                case InnerNumber.Integer:
                    parameterType = ParameterType.Integer;
                    return reader.ReadContentsAsInt64();
                case InnerNumber.Octetstring:
                    parameterType = ParameterType.Octets;
                    return reader.ReadContentsAsByteArray();
                case InnerNumber.Real:
                    parameterType = ParameterType.Real;
                    return reader.ReadContentsAsDouble();
                default:
                    parameterType = ParameterType.String;
                    return reader.AssertAndReadContentsAsString();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Method is not public, CA bug?")]
        internal sealed override void WriteValue(EmberWriter writer, object value)
        {
            switch (GetTypeToWrite(this.Type, value))
            {
                case ParameterType.Integer:
                case ParameterType.Enum:
                    writer.WriteValue(GlowParameterContents.Value.OuterId, (long)value);
                    break;
                case ParameterType.Real:
                    writer.WriteValue(GlowParameterContents.Value.OuterId, (double)value);
                    break;
                case ParameterType.String:
                    writer.WriteValue(GlowParameterContents.Value.OuterId, (string)value);
                    break;
                case ParameterType.Boolean:
                    writer.WriteValue(GlowParameterContents.Value.OuterId, (bool)value);
                    break;
                default:
                    writer.WriteValue(GlowParameterContents.Value.OuterId, (byte[])value);
                    break;
            }
        }

        internal sealed override object GetMinimum() => this.minimum;

        internal sealed override void SetMinimum(object value) => this.SetValue(ref this.minimum, value, "Minimum");

        internal sealed override object GetMaximum() => this.maximum;

        internal sealed override void SetMaximum(object value) => this.SetValue(ref this.maximum, value, "Maximum");

        internal sealed override object AssertValueType(object value)
        {
            try
            {
                switch (this.Type)
                {
                    case ParameterType.Integer:
                    case ParameterType.Enum:
                        return (long?)value;
                    case ParameterType.Real:
                        return (double?)value;
                    case ParameterType.String:
                        return (string)value;
                    case ParameterType.Boolean:
                        return (bool?)value;
                    case ParameterType.Trigger:
                        return value;
                    default:
                        return (byte[])value;
                }
            }
            catch (InvalidCastException ex)
            {
                throw new ArgumentException(
                    "The type of value does not match the type of the parameter.", nameof(value), ex);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static ParameterType GetTypeToWrite(ParameterType parameterType, object value)
        {
            if (parameterType != ParameterType.Trigger)
            {
                return parameterType;
            }

            var type = value.GetType();

            if (type == typeof(long))
            {
                return ParameterType.Integer;
            }
            else if (type == typeof(double))
            {
                return ParameterType.Real;
            }
            else if (type == typeof(string))
            {
                return ParameterType.String;
            }
            else if (type == typeof(bool))
            {
                return ParameterType.Boolean;
            }
            else
            {
                return ParameterType.Octets;
            }
        }

        private object minimum;
        private object maximum;
        private int? factor;
        private string formula;
        private IReadOnlyList<KeyValuePair<string, int>> enumMap;
    }
}
