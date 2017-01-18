////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.ComponentModel
{
    using System;
    using Reflection;

    /// <summary>Provides data for the <see cref="Binding{T, U, V, W}.ChangeOriginatedAtSource"/> and
    /// <see cref="Binding{T, U, V, W}.ChangeOriginatedAtTarget"/> events.</summary>
    /// <typeparam name="TOwner">The type of the owner object.</typeparam>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <threadsafety static="true" instance="false"/>
    public sealed class ChangeOriginatedAtEventArgs<TOwner, TProperty> : EventArgs
    {
        /// <summary>Gets the property where the change originated at.</summary>
        public IProperty<TOwner, TProperty> Property { get; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal ChangeOriginatedAtEventArgs(IProperty<TOwner, TProperty> property)
        {
            this.Property = property;
        }
    }
}
