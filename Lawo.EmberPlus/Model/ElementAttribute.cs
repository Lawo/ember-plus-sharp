////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model
{
    using System;

    /// <summary>Provides additional information for a property representing an Ember+ element, as defined in the
    /// <a href="http://ember-plus.googlecode.com/svn/trunk/documentation/Ember+%20Documentation.pdf">Ember+
    /// Specification</a>.</summary>
    /// <remarks>
    /// <para>This attribute only needs to be applied to properties where the property name is not identical to the
    /// Glow identifier or if the element is optional.</para>
    /// <para><b>Thread Safety</b>: Any public static members of this type are thread safe. Any instance members are not
    /// guaranteed to be thread safe.</para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ElementAttribute : Attribute
    {
        private string identifier;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets or sets the identifier of the element represented by the property carrying this attribute.
        /// </summary>
        /// <exception cref="ArgumentNullException">Attempted to set the value <c>null</c>.</exception>
        public string Identifier
        {
            get
            {
                return this.identifier;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                this.identifier = value;
            }
        }

        /// <summary>Gets or sets a value indicating whether the element represented by the property carrying this
        /// attribute is optional.</summary>
        /// <value>The value <c>true</c> if the element is optional; otherwise <c>false</c>.</value>
        public bool IsOptional { get; set; }
    }
}
