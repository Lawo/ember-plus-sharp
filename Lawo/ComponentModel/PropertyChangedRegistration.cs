////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.ComponentModel
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using Reflection;

    /// <summary>Represents multiple subscriptions to <see cref="INotifyPropertyChanged.PropertyChanged"/>.</summary>
    /// <threadsafety static="true" instance="false"/>
    public sealed class PropertyChangedRegistration : IDisposable
    {
        /// <summary>Initializes a new instance of the <see cref="PropertyChangedRegistration"/> class.</summary>
        /// <param name="handler">The handler to which all change notifications are forwarded.</param>
        /// <param name="properties">The properties for which change notifications should be forwarded.</param>
        /// <exception cref="ArgumentException">One or more elements of <paramref name="properties"/> equal <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="handler"/> and/or <paramref name="properties"/>
        /// equal <c>null</c>.</exception>
        /// <remarks>After construction, each change to one of the properties in <paramref name="properties"/> is
        /// forwarded to <paramref name="handler"/> until <see cref="Dispose"/> is called.</remarks>
        public PropertyChangedRegistration(
            PropertyChangedEventHandler handler, params IProperty<INotifyPropertyChanged>[] properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            if (Array.IndexOf(properties, null) >= 0)
            {
                throw new ArgumentException("Array elements cannot be null.", nameof(properties));
            }

            this.handler = handler ?? throw new ArgumentNullException(nameof(handler));
            this.propertyNames = properties.ToLookup(p => p.Owner, p => p.PropertyInfo.Name);

            foreach (var grouping in this.propertyNames)
            {
                grouping.Key.PropertyChanged += this.OnPropertyChanged;
            }
        }

        /// <summary>Stops forwarding change notifications.</summary>
        /// <remarks>If the registration is intended to be permanent it is permissible to to never call
        /// <see cref="Dispose"/>.</remarks>
        public void Dispose()
        {
            foreach (var grouping in this.propertyNames)
            {
                grouping.Key.PropertyChanged -= this.OnPropertyChanged;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private readonly PropertyChangedEventHandler handler;
        private readonly ILookup<INotifyPropertyChanged, string> propertyNames;

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this.propertyNames[(INotifyPropertyChanged)sender].Contains(e.PropertyName))
            {
                this.handler(sender, e);
            }
        }
    }
}
