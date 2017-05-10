////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.ComponentModel
{
    using System;
    using System.ComponentModel;
    using Reflection;

    /// <summary>Provides a method to create a trigger.</summary>
    /// <threadsafety static="true" instance="false"/>
    public static class Trigger
    {
        /// <summary>Creates a trigger such that <paramref name="handler"/> is called whenever
        /// <see cref="IProperty{T, U}.Value"/> has changed.</summary>
        /// <typeparam name="TOwner">The type of the owner object.</typeparam>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="property"/> and/or
        /// <paramref name="handler"/> equal <c>null</c>.</exception>
        /// <remarks>
        /// <para>Call <see cref="IDisposable.Dispose"/> on the returned object to stop having
        /// <paramref name="handler"/> called.</para>
        /// <para>If the trigger is intended to be permanent it is permissible to to never call
        /// <see cref="IDisposable.Dispose"/>.</para>
        /// </remarks>
        public static IDisposable Create<TOwner, TProperty>(
            IProperty<TOwner, TProperty> property, Action<IProperty<TOwner, TProperty>> handler)
            where TOwner : INotifyPropertyChanged
        {
            return new Forwarder<TOwner, TProperty>(
                property ?? throw new ArgumentNullException(nameof(property)),
                handler ?? throw new ArgumentNullException(nameof(handler)));
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private sealed class Forwarder<TOwner, TProperty> : IDisposable
            where TOwner : INotifyPropertyChanged
        {
            public void Dispose() => this.property.Owner.PropertyChanged -= this.OnPropertyChanged;

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////

            internal Forwarder(IProperty<TOwner, TProperty> property, Action<IProperty<TOwner, TProperty>> handler)
            {
                this.property = property;
                this.handler = handler;
                this.property.Owner.PropertyChanged += this.OnPropertyChanged;
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////

            private readonly IProperty<TOwner, TProperty> property;
            private readonly Action<IProperty<TOwner, TProperty>> handler;

            private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == this.property.PropertyInfo.Name)
                {
                    this.handler(this.property);
                }
            }
        }
    }
}
