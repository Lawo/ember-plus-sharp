////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System;
    using System.Linq;
    using System.Reflection;

    using Ember;
    using Glow;

    internal sealed class ValueWriter<T>
    {
        internal ValueWriter(T value)
        {
            this.value = value;
        }

        internal void WriteValue(EmberWriter writer) => WriteValueCore(writer, GlowTuple.Value.OuterId, this.value);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static readonly Action<EmberWriter, EmberId, T> WriteValueCore = GetWriteValueCore();

        private static Action<EmberWriter, EmberId, T> GetWriteValueCore()
        {
            var method = typeof(EmberWriter).GetTypeInfo().GetDeclaredMethods("WriteValue").FirstOrDefault(
                i => i.GetParameters()[1].ParameterType == typeof(T));
            return (Action<EmberWriter, EmberId, T>)method.CreateDelegate(typeof(Action<EmberWriter, EmberId, T>));
        }

        private readonly T value;
    }
}
