////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
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
