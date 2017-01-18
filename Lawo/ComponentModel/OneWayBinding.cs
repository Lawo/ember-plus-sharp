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

    /// <summary>Provides methods to create one-way <see cref="Binding{T, U, V, W}"/> instances.</summary>
    /// <threadsafety static="true" instance="false"/>
    public static class OneWayBinding
    {
        /// <summary>Returns the return value of <see cref="Create{T, U, V, W}">Create(<paramref name="source"/>,
        /// v => v, <paramref name="target"/>)</see>.
        /// </summary>
        /// <typeparam name="TSourceOwner">The type of the object owning the source property.</typeparam>
        /// <typeparam name="TTargetOwner">The type of the object owning the target property.</typeparam>
        /// <typeparam name="TProperty">The type of the source and target properties.</typeparam>
        public static Binding<TSourceOwner, TProperty, TTargetOwner, TProperty> Create<TSourceOwner, TTargetOwner, TProperty>(
            IProperty<TSourceOwner, TProperty> source, IProperty<TTargetOwner, TProperty> target)
            where TSourceOwner : INotifyPropertyChanged
            where TTargetOwner : INotifyPropertyChanged
        {
            return Create(source, v => v, target);
        }

        /// <summary>Creates a one-way binding between <paramref name="source"/> and <paramref name="target"/>.
        /// </summary>
        /// <remarks>Firstly, the value of <paramref name="target"/>.<see cref="IProperty{T, U}.Value"/> is set to the
        /// one of <paramref name="toTarget"/>(<paramref name="source"/>.<see cref="IProperty{T, U}.Value"/>). Secondly,
        /// a handler is added to the <see cref="INotifyPropertyChanged.PropertyChanged"/> event of the source property.
        /// After establishing that the source property has been changed, the handler sets the value of the target
        /// property to the (appropriately converted) value of the source property.</remarks>
        /// <typeparam name="TSourceOwner">The type of the object owning the source property.</typeparam>
        /// <typeparam name="TSource">The type of the source property.</typeparam>
        /// <typeparam name="TTargetOwner">The type of the object owning the target property.</typeparam>
        /// <typeparam name="TTarget">The type of the target property.</typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="source"/>, <paramref name="toTarget"/>,
        /// <paramref name="target"/>.</exception>
        public static Binding<TSourceOwner, TSource, TTargetOwner, TTarget> Create<TSourceOwner, TSource, TTargetOwner, TTarget>(
            IProperty<TSourceOwner, TSource> source,
            Func<TSource, TTarget> toTarget,
            IProperty<TTargetOwner, TTarget> target)
            where TSourceOwner : INotifyPropertyChanged
            where TTargetOwner : INotifyPropertyChanged
        {
            return new Binding<TSourceOwner, TSource, TTargetOwner, TTarget>(source, toTarget, target, null);
        }
    }
}
