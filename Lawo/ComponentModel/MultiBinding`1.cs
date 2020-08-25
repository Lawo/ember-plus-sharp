////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.ComponentModel
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    using Reflection;

    /// <summary>Represents a one-way binding between a single target property and one or more source properties.
    /// </summary>
    /// <typeparam name="T">The type of the target property.</typeparam>
    /// <seealso cref="MultiBinding"/>
    /// <threadsafety static="true" instance="false"/>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Somehow the CA dictionary does not seem to work here.")]
    public sealed class MultiBinding<T> : IDisposable
    {
        /// <summary>Stops setting the target property whenever one of the source properties changes.</summary>
        /// <remarks>If the binding is intended to be permanent it is permissible to to never call
        /// <see cref="Dispose"/>.</remarks>
        public void Dispose() => this.propertyChangedRegistration.Dispose();

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal MultiBinding(
            IProperty<INotifyPropertyChanged>[] sources, Func<T> toTarget, IProperty<object, T> target)
        {
            var nullIndex = Array.IndexOf(sources, null);

            if (nullIndex >= 0)
            {
                throw new ArgumentNullException(
                    string.Format(CultureInfo.InvariantCulture, "s{0}", nullIndex + 1));
            }

            this.target = target ?? throw new ArgumentNullException(nameof(target));
            this.calculate = toTarget;
            this.propertyChangedRegistration = new PropertyChangedRegistration(this.OnPropertyChanged, sources);
            this.target.Value = this.calculate();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private readonly IProperty<object, T> target;
        private readonly Func<T> calculate;
        private readonly PropertyChangedRegistration propertyChangedRegistration;

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e) =>
            this.target.Value = this.calculate();
    }
}
