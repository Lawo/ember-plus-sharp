////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.Reflection
{
    using System;
    using System.Reflection;

    /// <summary>Provides the means to get and set the value of a property as well as access to the owner object and
    /// property metadata.</summary>
    /// <typeparam name="TOwner">The type of the owner object.</typeparam>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    public interface IProperty<out TOwner, TProperty> : IProperty<TOwner>
    {
        /// <summary>Gets or sets the value of the property.</summary>
        /// <exception cref="InvalidOperationException">Attempted to set a value and the represented property does not
        /// have a setter.</exception>
        TProperty Value { get; set; }
    }
}
