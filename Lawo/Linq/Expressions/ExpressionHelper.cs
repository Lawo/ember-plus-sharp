////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.Linq.Expressions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>Provides helper methods for expressions.</summary>
    /// <threadsafety static="true" instance="false"/>
    public static class ExpressionHelper
    {
        /// <summary>Returns the <see cref="PropertyInfo"/> associated with the property identified by
        /// <paramref name="getPropertyExpression"/>.</summary>
        /// <typeparam name="TOwner">The type of the owner of the property.</typeparam>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="getPropertyExpression">An expression calling the getter of a property, for example
        /// <c>(Type t) => t.FullName</c>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="getPropertyExpression"/> equals <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">The expression passed for <paramref name="getPropertyExpression"/> does
        /// not return the value of a property.</exception>
        /// <remarks>
        /// <para>Both <typeparamref name="TOwner"/> and <typeparamref name="TProperty"/> are automatically
        /// deduced from the expression passed for <paramref name="getPropertyExpression"/> and therefore do not need to
        /// be specified explicitly.</para>
        /// <para>This method is helpful whenever code needs to retrieve the <see cref="PropertyInfo"/> associated with
        /// the property of a given type.</para>
        /// <para>It is superior to <see cref="TypeInfo.GetDeclaredProperty(string)"/>, because it avoids duplicating
        /// the name of the property in a string. Such duplicated names are often forgotten during refactoring and the
        /// resulting bugs can only be found during testing. The specified expression is a language element that is
        /// checked for consistency at compile time and is automatically changed during refactoring.</para>
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "We want to make sure there's one parameter and a return value.")]
        public static PropertyInfo GetPropertyInfo<TOwner, TProperty>(
            Expression<Func<TOwner, TProperty>> getPropertyExpression)
        {
            return GetPropertyInfoCore(getPropertyExpression);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static PropertyInfo GetPropertyInfoCore(LambdaExpression getPropertyExpression)
        {
            if (getPropertyExpression == null)
            {
                throw new ArgumentNullException(nameof(getPropertyExpression));
            }

            var memberExpression = getPropertyExpression.Body as MemberExpression;
            PropertyInfo propertyInfo;

            if ((memberExpression == null) || ((propertyInfo = memberExpression.Member as PropertyInfo) == null))
            {
                throw new ArgumentException("Does not return the value of a property.", nameof(getPropertyExpression));
            }

            return propertyInfo;
        }
    }
}
