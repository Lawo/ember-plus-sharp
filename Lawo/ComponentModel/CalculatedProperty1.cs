////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.ComponentModel
{
    using System;
    using System.ComponentModel;
    using Lawo.Reflection;

    /// <summary>Represents a calculated property, the value of which is calculated from a number of source properties.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <seealso cref="CalculatedProperty"/>
    public sealed class CalculatedProperty<T> : IDisposable
    {
        private readonly MultiBinding<T> binding;
        private readonly NotifyPropertyChanged owner;
        private readonly PropertyChangedEventArgs args;
        private T targetValue;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Stops updating <see cref="Value"/> whenever one of the source properties changes.</summary>
        /// <remarks>If the dependency is intended to be permanent it is permissible to to never call
        /// <see cref="Dispose"/>.</remarks>
        public void Dispose()
        {
            this.binding.Dispose();
        }

        /// <summary>Gets the value of the calculated property.</summary>
        public T Value
        {
            get
            {
                return this.targetValue;
            }

            private set
            {
                if (!GenericCompare.Equals(this.targetValue, value))
                {
                    this.targetValue = value;
                    this.owner.OnPropertyChanged(this.args);
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal CalculatedProperty(
            IProperty<NotifyPropertyChanged> target,
            Func<IProperty<CalculatedProperty<T>, T>, MultiBinding<T>> createBinding)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            this.owner = target.Owner;
            this.args = new PropertyChangedEventArgs(target.PropertyInfo.Name);
            this.binding = createBinding(this.GetProperty(o => o.Value));
        }
    }
}
