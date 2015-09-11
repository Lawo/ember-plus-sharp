////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
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
        private readonly IProperty<TOwner, TProperty> property;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets the property where the change originated at.</summary>
        public IProperty<TOwner, TProperty> Property
        {
            get { return this.property; }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal ChangeOriginatedAtEventArgs(IProperty<TOwner, TProperty> property)
        {
            this.property = property;
        }
    }
}
