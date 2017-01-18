////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.Reflection
{
    using System.Reflection;

    /// <summary>Provides access to the owner object and metadata of a property.</summary>
    /// <typeparam name="TOwner">The type of the owner object.</typeparam>
    public interface IProperty<out TOwner>
    {
        /// <summary>Gets the owner object, cannot be <c>null</c>.</summary>
        TOwner Owner { get; }

        /// <summary>Gets the <see cref="PropertyInfo"/> object identifying the property, cannot be <c>null</c>.
        /// </summary>
        PropertyInfo PropertyInfo { get; }
    }
}
