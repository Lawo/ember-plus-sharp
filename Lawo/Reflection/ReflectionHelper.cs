////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.Reflection
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using Linq.Expressions;

    /// <summary>Provides methods to create <see cref="IProperty{T, U}"/> instances.</summary>
    /// <threadsafety static="true" instance="false"/>
    public static class ReflectionHelper
    {
        /// <summary>Creates an instance implementing <see cref="IProperty{T, U}"/>.</summary>
        /// <typeparam name="TOwner">The type of <paramref name="owner"/>.</typeparam>
        /// <typeparam name="TProperty">The type of the property identified by <paramref name="getPropertyExpression"/>.
        /// </typeparam>
        /// <param name="owner">The owner object.</param>
        /// <param name="getPropertyExpression">An expression calling the getter of a property, for example
        /// <c>s => s.Length</c>.</param>
        /// <exception cref="ArgumentException">The expression passed for <paramref name="getPropertyExpression"/> does
        /// not return the value of a property.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="owner"/> and/or
        /// <paramref name="getPropertyExpression"/> equal <c>null</c>.</exception>
        public static IProperty<TOwner, TProperty> GetProperty<TOwner, TProperty>(
            this TOwner owner, Expression<Func<TOwner, TProperty>> getPropertyExpression)
        {
            return new PropertyImpl<TOwner, TProperty>(owner, getPropertyExpression);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private sealed class PropertyImpl<TOwner, TProperty> : IProperty<TOwner, TProperty>
        {
            public TOwner Owner => this.owner;

            public PropertyInfo PropertyInfo => this.propertyInfo;

            public TProperty Value
            {
                get { return this.Getter(this.owner); }
                set { this.Setter(this.owner, value); }
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////

            internal PropertyImpl(TOwner owner, Expression<Func<TOwner, TProperty>> getPropertyExpression)
            {
                if (owner == null)
                {
                    throw new ArgumentNullException(nameof(owner));
                }

                this.owner = owner;
                this.propertyInfo = ExpressionHelper.GetPropertyInfo(getPropertyExpression);
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////

            private readonly TOwner owner;
            private readonly PropertyInfo propertyInfo;
            private Func<TOwner, TProperty> getter;
            private Action<TOwner, TProperty> setter;

            private Func<TOwner, TProperty> Getter
            {
                get
                {
                    if (this.getter == null)
                    {
                        var ownerParam = Expression.Parameter(typeof(TOwner));
                        this.getter = Expression.Lambda<Func<TOwner, TProperty>>(
                            Expression.Property(ownerParam, this.propertyInfo), ownerParam).Compile();
                    }

                    return this.getter;
                }
            }

            private Action<TOwner, TProperty> Setter
            {
                get
                {
                    if (this.setter == null)
                    {
                        var ownerParam = Expression.Parameter(typeof(TOwner));
                        var valueParam = Expression.Parameter(typeof(TProperty));
                        var property = Expression.Property(ownerParam, this.propertyInfo);
                        BinaryExpression assignment;

                        try
                        {
                            assignment = Expression.Assign(property, valueParam);
                        }
                        catch (ArgumentException ex)
                        {
                            throw new InvalidOperationException("The represented property does not have a setter.", ex);
                        }

                        this.setter = Expression.Lambda<Action<TOwner, TProperty>>(
                            assignment, ownerParam, valueParam).Compile();
                    }

                    return this.setter;
                }
            }
        }
    }
}
