﻿////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Lawo.EmberPlus.Ember;
    using Lawo.EmberPlus.Glow;

    internal sealed class ValueWriter<T>
    {
        private static readonly Action<EmberWriter, EmberId, T> WriteValueCore = GetWriteValueCore();
        private readonly T value;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal ValueWriter(T value)
        {
            this.value = value;
        }

        internal void WriteValue(EmberWriter writer)
        {
            WriteValueCore(writer, GlowTuple.Value.OuterId, this.value);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static Action<EmberWriter, EmberId, T> GetWriteValueCore()
        {
            var method = typeof(EmberWriter).GetTypeInfo().GetDeclaredMethods("WriteValue").FirstOrDefault(
                i => i.GetParameters()[1].ParameterType == typeof(T));
            return (Action<EmberWriter, EmberId, T>)method.CreateDelegate(typeof(Action<EmberWriter, EmberId, T>));
        }
    }
}