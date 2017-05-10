////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System;

    /// <summary>Provides additional information for a property representing an element in the object tree accessible
    /// through <see cref="Consumer{T}.Root">Consumer&lt;TRoot&gt;.Root</see>.</summary>
    /// <remarks>This attribute only needs to be applied to properties where the property name is not identical to the
    /// Glow identifier or if the element is optional.</remarks>
    /// <threadsafety static="true" instance="false"/>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ElementAttribute : Attribute
    {
        /// <summary>Gets or sets the identifier of the element represented by the property carrying this attribute.
        /// </summary>
        /// <exception cref="ArgumentNullException">Attempted to set the value <c>null</c>.</exception>
        public string Identifier
        {
            get => this.identifier;
            set => this.identifier = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>Gets or sets a value indicating whether the element represented by the property carrying this
        /// attribute is optional.</summary>
        /// <value>The value <c>true</c> if the element is optional; otherwise <c>false</c>.</value>
        public bool IsOptional { get; set; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private string identifier;
    }
}
