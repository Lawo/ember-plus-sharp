////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.Reflection
{
    using System;

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
