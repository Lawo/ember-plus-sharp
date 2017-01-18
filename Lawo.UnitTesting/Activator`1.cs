////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright 2008-2012 Andreas Huber Doenni, original from http://phuse.codeplex.com.
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.UnitTesting
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;

    /// <summary>Facilitates construction of a new object via reflection.</summary>
    /// <typeparam name="T">The type of the object to create.</typeparam>
    /// <threadsafety static="true" instance="false"/>
    public static class Activator<T>
    {
        /// <summary>Creates a new instance by passing <paramref name="p1"/> to a matching constructor of
        /// <typeparamref name="T"/>.</summary>
        /// <typeparam name="TP1">The type of the constructor parameter.</typeparam>
        /// <exception cref="ArgumentException"><typeparamref name="T"/> does not have a constructor that accepts
        /// <paramref name="p1"/>.</exception>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "This is a generic parameter.")]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "There's no other way to achieve argument type deduction.")]
        public static T CreateInstance<TP1>(TP1 p1) => CreateInstanceImpl(new[] { typeof(TP1) }, new object[] { p1 });

        /// <summary>Creates a new instance by passing <paramref name="p1"/> and <paramref name="p2"/> to a matching
        /// constructor of <typeparamref name="T"/>.</summary>
        /// <typeparam name="TP1">The type of the first constructor parameter.</typeparam>
        /// <typeparam name="TP2">The type of the second constructor parameter.</typeparam>
        /// <exception cref="ArgumentException"><typeparamref name="T"/> does not have a constructor that accepts
        /// <paramref name="p1"/> and <paramref name="p2"/>.</exception>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "These are generic parameters.")]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "There's no other way to achieve argument type deduction.")]
        public static T CreateInstance<TP1, TP2>(TP1 p1, TP2 p2) =>
            CreateInstanceImpl(new[] { typeof(TP1), typeof(TP2) }, new object[] { p1, p2 });

        /// <summary>Creates a new instance by passing <paramref name="args"/> to a matching constructor of
        /// <typeparamref name="T"/>.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="args"/> is <c>null</c>; <b>or</b> one of the
        /// elements of <paramref name="args"/> is null.</exception>
        /// <exception cref="ArgumentException"><typeparamref name="T"/> does not have a constructor that accepts
        /// <paramref name="args"/>.</exception>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "There's no other way to achieve argument type deduction.")]
        public static T CreateInstance(params object[] args) =>
            CreateInstanceImpl(args.Select(obj => obj?.GetType()).ToArray(), args);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static T CreateInstanceImpl(Type[] ctorArgTypes, object[] ctorArgs)
        {
            var info = typeof(T).GetTypeInfo().DeclaredConstructors.FirstOrDefault(
                c => c.GetParameters().Select(p => p.ParameterType).SequenceEqual(ctorArgTypes));

            if (info == null)
            {
                const string Format = "{0} does not have a constructor accepting the passed arguments.";
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Format, typeof(T)));
            }

            try
            {
                return (T)info.Invoke(ctorArgs);
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }
    }
}
