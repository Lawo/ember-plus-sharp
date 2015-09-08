////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.ComponentModel
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using Lawo.Reflection;

    /// <summary>Provides a generic implementation for <see cref="INotifyPropertyChanged"/>.</summary>
    /// <remarks>
    /// <para><b>Thread Safety</b>: Any public static members of this type are thread safe. Any instance members are not
    /// guaranteed to be thread safe.</para>
    /// </remarks>
    public abstract class NotifyPropertyChanged : INotifyPropertyChanged
    {
        /// <summary>See <see cref="INotifyPropertyChanged.PropertyChanged"/>.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Invoked immediately after the value of a property has been changed.</summary>
        /// <param name="e">Event data that can be examined by overriding code. The event data carries the name of the
        /// changed property.</param>
        /// <remarks>Overrides must call this method if a change needs to be published through the
        /// <see cref="PropertyChanged"/> event.</remarks>
        protected internal virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var handler = this.PropertyChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Initializes a new instance of the <see cref="NotifyPropertyChanged"/> class.</summary>
        protected NotifyPropertyChanged()
        {
        }

        /// <summary>Sets <paramref name="field"/> to <paramref name="newValue"/> and calls
        /// <see cref="OnPropertyChanged"/> if their values were not equal.</summary>
        /// <typeparam name="T">The type of <paramref name="field"/> and <paramref name="newValue"/>.</typeparam>
        /// <param name="field">The field to set.</param>
        /// <param name="newValue">The new value to set.</param>
        /// <param name="propertyName">The name of the property from which this method is called. Due to the use of the
        /// <see cref="CallerMemberNameAttribute"/> an argument for this parameter only needs to be specified if this
        /// method is not directly called from a property.</param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The CallerMemberName semantics leave no other way.")]
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", Justification = "There's no other easy way to have a field set by this method.")]
        protected bool SetValue<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            var result = !GenericCompare.Equals(field, newValue);

            if (result)
            {
                field = newValue;
                this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
            }

            return result;
        }
    }
}
