////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.ComponentModel
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Lawo.Reflection;

    /// <summary>Represents multiple subscriptions to <see cref="INotifyPropertyChanged.PropertyChanged"/>.</summary>
    /// <remarks>
    /// <para><b>Thread Safety</b>: Any public static members of this type are thread safe. Any instance members are not
    /// guaranteed to be thread safe.</para>
    /// </remarks>
    public sealed class PropertyChangedRegistration : IDisposable
    {
        private readonly PropertyChangedEventHandler handler;
        private readonly ILookup<INotifyPropertyChanged, string> propertyNames;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

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
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            if (properties == null)
            {
                throw new ArgumentNullException("properties");
            }

            if (Array.IndexOf(properties, null) >= 0)
            {
                throw new ArgumentException("Array elements cannot be null.", "properties");
            }

            this.handler = handler;
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

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this.propertyNames[(INotifyPropertyChanged)sender].Contains(e.PropertyName))
            {
                this.handler(sender, e);
            }
        }
    }
}
