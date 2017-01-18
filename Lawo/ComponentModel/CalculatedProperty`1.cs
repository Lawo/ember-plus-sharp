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

    /// <summary>Represents a calculated property, the value of which is calculated from a number of source properties.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <seealso cref="CalculatedProperty"/>
    /// <threadsafety static="true" instance="false"/>
    public sealed class CalculatedProperty<T> : IDisposable
    {
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

        /// <summary>Stops updating <see cref="Value"/> whenever one of the source properties changes.</summary>
        /// <remarks>If the dependency is intended to be permanent it is permissible to to never call
        /// <see cref="Dispose"/>.</remarks>
        public void Dispose() => this.binding.Dispose();

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal CalculatedProperty(
            IProperty<NotifyPropertyChanged> target,
            Func<IProperty<CalculatedProperty<T>, T>, MultiBinding<T>> createBinding)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            this.owner = target.Owner;
            this.args = new PropertyChangedEventArgs(target.PropertyInfo.Name);
            this.binding = createBinding(this.GetProperty(o => o.Value));
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private readonly MultiBinding<T> binding;
        private readonly NotifyPropertyChanged owner;
        private readonly PropertyChangedEventArgs args;
        private T targetValue;
    }
}
