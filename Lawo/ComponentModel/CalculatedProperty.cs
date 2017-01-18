////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.ComponentModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using Reflection;

    /// <summary>Provides methods to create <see cref="CalculatedProperty{T}"/> instances.</summary>
    /// <remarks>
    /// <para>Compared to the creation of a <see cref="MultiBinding{T}"/>, a <see cref="CalculatedProperty{T}"/>
    /// instance slightly simplifies the implementation of a calculated property in classes that derive from
    /// <see cref="NotifyPropertyChanged"/>. A <see cref="CalculatedProperty{T}"/> object is typically assigned to a
    /// readonly field of the class that contains the calculated property. The calculated property itself is implemented
    /// with a getter and no setter. The getter simply returns <see cref="CalculatedProperty{T}.Value"/> of the
    /// <see cref="CalculatedProperty{T}"/> field.</para>
    /// <para>When a <see cref="CalculatedProperty{T}"/> instance is created, its
    /// <see cref="CalculatedProperty{T}.Value"/> property is calculated from the values of the source
    /// properties. Whenever one of the source properties changes, <see cref="CalculatedProperty{T}.Value"/> is
    /// recalculated. If the new value differs from the old value, <see cref="INotifyPropertyChanged.PropertyChanged"/>
    /// is raised accordingly.</para>
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    public static class CalculatedProperty
    {
        /// <summary>Creates and returns a <see cref="CalculatedProperty{T}"/> instance.</summary>
        /// <typeparam name="T">The type of the source and target property.</typeparam>
        /// <exception cref="ArgumentNullException">At least one of the arguments is equal to <c>null</c>.</exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Requiring the derived type eliminates type mismatches.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Longer names would lead to unwieldy code.")]
        public static CalculatedProperty<T> Create<T>(
            IProperty<INotifyPropertyChanged, T> s1, IProperty<NotifyPropertyChanged, T> target)
        {
            return new CalculatedProperty<T>(target, t => MultiBinding.Create(s1, t));
        }

        /// <summary>Creates and returns a <see cref="CalculatedProperty{T}"/> instance.</summary>
        /// <typeparam name="TS1">The type of the first source property.</typeparam>
        /// <typeparam name="TTarget">The type of the target property.</typeparam>
        /// <exception cref="ArgumentNullException">At least one of the arguments is equal to <c>null</c>.</exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Requiring the derived type eliminates type mismatches.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Longer names would lead to unwieldy code.")]
        public static CalculatedProperty<TTarget> Create<TS1, TTarget>(
            IProperty<INotifyPropertyChanged, TS1> s1,
            Func<TS1, TTarget> toTarget,
            IProperty<NotifyPropertyChanged, TTarget> target)
        {
            return new CalculatedProperty<TTarget>(target, t => MultiBinding.Create(s1, toTarget, t));
        }

        /// <summary>Creates and returns a <see cref="CalculatedProperty{T}"/> instance.</summary>
        /// <typeparam name="TS1">The type of the first source property.</typeparam>
        /// <typeparam name="TS2">The type of the second source property.</typeparam>
        /// <typeparam name="TTarget">The type of the target property.</typeparam>
        /// <exception cref="ArgumentNullException">At least one of the arguments is equal to <c>null</c>.</exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Requiring the derived type eliminates type mismatches.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Longer names would lead to unwieldy code.")]
        public static CalculatedProperty<TTarget> Create<TS1, TS2, TTarget>(
            IProperty<INotifyPropertyChanged, TS1> s1,
            IProperty<INotifyPropertyChanged, TS2> s2,
            Func<TS1, TS2, TTarget> toTarget,
            IProperty<NotifyPropertyChanged, TTarget> target)
        {
            return new CalculatedProperty<TTarget>(target, t => MultiBinding.Create(s1, s2, toTarget, t));
        }

        /// <summary>Creates and returns a <see cref="CalculatedProperty{T}"/> instance.</summary>
        /// <typeparam name="TS1">The type of the first source property.</typeparam>
        /// <typeparam name="TS2">The type of the second source property.</typeparam>
        /// <typeparam name="TS3">The type of the third source property.</typeparam>
        /// <typeparam name="TTarget">The type of the target property.</typeparam>
        /// <exception cref="ArgumentNullException">At least one of the arguments is equal to <c>null</c>.</exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Requiring the derived type eliminates type mismatches.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Longer names would lead to unwieldy code.")]
        public static CalculatedProperty<TTarget> Create<TS1, TS2, TS3, TTarget>(
            IProperty<INotifyPropertyChanged, TS1> s1,
            IProperty<INotifyPropertyChanged, TS2> s2,
            IProperty<INotifyPropertyChanged, TS3> s3,
            Func<TS1, TS2, TS3, TTarget> toTarget,
            IProperty<NotifyPropertyChanged, TTarget> target)
        {
            return new CalculatedProperty<TTarget>(target, t => MultiBinding.Create(s1, s2, s3, toTarget, t));
        }

        /// <summary>Creates and returns a <see cref="CalculatedProperty{T}"/> instance.</summary>
        /// <typeparam name="TS1">The type of the first source property.</typeparam>
        /// <typeparam name="TS2">The type of the second source property.</typeparam>
        /// <typeparam name="TS3">The type of the third source property.</typeparam>
        /// <typeparam name="TS4">The type of the fourth source property.</typeparam>
        /// <typeparam name="TTarget">The type of the target property.</typeparam>
        /// <exception cref="ArgumentNullException">At least one of the arguments is equal to <c>null</c>.</exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Requiring the derived type eliminates type mismatches.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Longer names would lead to unwieldy code.")]
        public static CalculatedProperty<TTarget> Create<TS1, TS2, TS3, TS4, TTarget>(
            IProperty<INotifyPropertyChanged, TS1> s1,
            IProperty<INotifyPropertyChanged, TS2> s2,
            IProperty<INotifyPropertyChanged, TS3> s3,
            IProperty<INotifyPropertyChanged, TS4> s4,
            Func<TS1, TS2, TS3, TS4, TTarget> toTarget,
            IProperty<NotifyPropertyChanged, TTarget> target)
        {
            return new CalculatedProperty<TTarget>(target, t => MultiBinding.Create(s1, s2, s3, s4, toTarget, t));
        }

        /// <summary>Creates and returns a <see cref="CalculatedProperty{T}"/> instance.</summary>
        /// <typeparam name="TS1">The type of the first source property.</typeparam>
        /// <typeparam name="TS2">The type of the second source property.</typeparam>
        /// <typeparam name="TS3">The type of the third source property.</typeparam>
        /// <typeparam name="TS4">The type of the fourth source property.</typeparam>
        /// <typeparam name="TS5">The type of the fifth source property.</typeparam>
        /// <typeparam name="TTarget">The type of the target property.</typeparam>
        /// <exception cref="ArgumentNullException">At least one of the arguments is equal to <c>null</c>.</exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Requiring the derived type eliminates type mismatches.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Longer names would lead to unwieldy code.")]
        public static CalculatedProperty<TTarget> Create<TS1, TS2, TS3, TS4, TS5, TTarget>(
            IProperty<INotifyPropertyChanged, TS1> s1,
            IProperty<INotifyPropertyChanged, TS2> s2,
            IProperty<INotifyPropertyChanged, TS3> s3,
            IProperty<INotifyPropertyChanged, TS4> s4,
            IProperty<INotifyPropertyChanged, TS5> s5,
            Func<TS1, TS2, TS3, TS4, TS5, TTarget> toTarget,
            IProperty<NotifyPropertyChanged, TTarget> target)
        {
            return new CalculatedProperty<TTarget>(target, t => MultiBinding.Create(s1, s2, s3, s4, s5, toTarget, t));
        }

        /// <summary>Creates and returns a <see cref="CalculatedProperty{T}"/> instance.</summary>
        /// <typeparam name="TS1">The type of the first source property.</typeparam>
        /// <typeparam name="TS2">The type of the second source property.</typeparam>
        /// <typeparam name="TS3">The type of the third source property.</typeparam>
        /// <typeparam name="TS4">The type of the fourth source property.</typeparam>
        /// <typeparam name="TS5">The type of the fifth source property.</typeparam>
        /// <typeparam name="TS6">The type of the sixth source property.</typeparam>
        /// <typeparam name="TTarget">The type of the target property.</typeparam>
        /// <exception cref="ArgumentNullException">At least one of the arguments is equal to <c>null</c>.</exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Requiring the derived type eliminates type mismatches.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Longer names would lead to unwieldy code.")]
        public static CalculatedProperty<TTarget> Create<TS1, TS2, TS3, TS4, TS5, TS6, TTarget>(
            IProperty<INotifyPropertyChanged, TS1> s1,
            IProperty<INotifyPropertyChanged, TS2> s2,
            IProperty<INotifyPropertyChanged, TS3> s3,
            IProperty<INotifyPropertyChanged, TS4> s4,
            IProperty<INotifyPropertyChanged, TS5> s5,
            IProperty<INotifyPropertyChanged, TS6> s6,
            Func<TS1, TS2, TS3, TS4, TS5, TS6, TTarget> toTarget,
            IProperty<NotifyPropertyChanged, TTarget> target)
        {
            return new CalculatedProperty<TTarget>(
                target, t => MultiBinding.Create(s1, s2, s3, s4, s5, s6, toTarget, t));
        }

        /// <summary>Creates and returns a <see cref="CalculatedProperty{T}"/> instance.</summary>
        /// <typeparam name="TS1">The type of the first source property.</typeparam>
        /// <typeparam name="TS2">The type of the second source property.</typeparam>
        /// <typeparam name="TS3">The type of the third source property.</typeparam>
        /// <typeparam name="TS4">The type of the fourth source property.</typeparam>
        /// <typeparam name="TS5">The type of the fifth source property.</typeparam>
        /// <typeparam name="TS6">The type of the sixth source property.</typeparam>
        /// <typeparam name="TS7">The type of the seventh source property.</typeparam>
        /// <typeparam name="TTarget">The type of the target property.</typeparam>
        /// <exception cref="ArgumentNullException">At least one of the arguments is equal to <c>null</c>.</exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Requiring the derived type eliminates type mismatches.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Longer names would lead to unwieldy code.")]
        public static CalculatedProperty<TTarget> Create<TS1, TS2, TS3, TS4, TS5, TS6, TS7, TTarget>(
            IProperty<INotifyPropertyChanged, TS1> s1,
            IProperty<INotifyPropertyChanged, TS2> s2,
            IProperty<INotifyPropertyChanged, TS3> s3,
            IProperty<INotifyPropertyChanged, TS4> s4,
            IProperty<INotifyPropertyChanged, TS5> s5,
            IProperty<INotifyPropertyChanged, TS6> s6,
            IProperty<INotifyPropertyChanged, TS7> s7,
            Func<TS1, TS2, TS3, TS4, TS5, TS6, TS7, TTarget> toTarget,
            IProperty<NotifyPropertyChanged, TTarget> target)
        {
            return new CalculatedProperty<TTarget>(
                target, t => MultiBinding.Create(s1, s2, s3, s4, s5, s6, s7, toTarget, t));
        }

        /// <summary>Creates and returns a <see cref="CalculatedProperty{T}"/> instance.</summary>
        /// <typeparam name="TS1">The type of the first source property.</typeparam>
        /// <typeparam name="TS2">The type of the second source property.</typeparam>
        /// <typeparam name="TS3">The type of the third source property.</typeparam>
        /// <typeparam name="TS4">The type of the fourth source property.</typeparam>
        /// <typeparam name="TS5">The type of the fifth source property.</typeparam>
        /// <typeparam name="TS6">The type of the sixth source property.</typeparam>
        /// <typeparam name="TS7">The type of the seventh source property.</typeparam>
        /// <typeparam name="TS8">The type of the eighth source property.</typeparam>
        /// <typeparam name="TTarget">The type of the target property.</typeparam>
        /// <exception cref="ArgumentNullException">At least one of the arguments is equal to <c>null</c>.</exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Requiring the derived type eliminates type mismatches.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Longer names would lead to unwieldy code.")]
        public static CalculatedProperty<TTarget> Create<TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TTarget>(
            IProperty<INotifyPropertyChanged, TS1> s1,
            IProperty<INotifyPropertyChanged, TS2> s2,
            IProperty<INotifyPropertyChanged, TS3> s3,
            IProperty<INotifyPropertyChanged, TS4> s4,
            IProperty<INotifyPropertyChanged, TS5> s5,
            IProperty<INotifyPropertyChanged, TS6> s6,
            IProperty<INotifyPropertyChanged, TS7> s7,
            IProperty<INotifyPropertyChanged, TS8> s8,
            Func<TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TTarget> toTarget,
            IProperty<NotifyPropertyChanged, TTarget> target)
        {
            return new CalculatedProperty<TTarget>(
                target, t => MultiBinding.Create(s1, s2, s3, s4, s5, s6, s7, s8, toTarget, t));
        }

        /// <summary>Creates and returns a <see cref="CalculatedProperty{T}"/> instance.</summary>
        /// <typeparam name="TS1">The type of the first source property.</typeparam>
        /// <typeparam name="TS2">The type of the second source property.</typeparam>
        /// <typeparam name="TS3">The type of the third source property.</typeparam>
        /// <typeparam name="TS4">The type of the fourth source property.</typeparam>
        /// <typeparam name="TS5">The type of the fifth source property.</typeparam>
        /// <typeparam name="TS6">The type of the sixth source property.</typeparam>
        /// <typeparam name="TS7">The type of the seventh source property.</typeparam>
        /// <typeparam name="TS8">The type of the eighth source property.</typeparam>
        /// <typeparam name="TS9">The type of the ninth source property.</typeparam>
        /// <typeparam name="TTarget">The type of the target property.</typeparam>
        /// <exception cref="ArgumentNullException">At least one of the arguments is equal to <c>null</c>.</exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Requiring the derived type eliminates type mismatches.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Longer names would lead to unwieldy code.")]
        public static CalculatedProperty<TTarget> Create<TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TTarget>(
            IProperty<INotifyPropertyChanged, TS1> s1,
            IProperty<INotifyPropertyChanged, TS2> s2,
            IProperty<INotifyPropertyChanged, TS3> s3,
            IProperty<INotifyPropertyChanged, TS4> s4,
            IProperty<INotifyPropertyChanged, TS5> s5,
            IProperty<INotifyPropertyChanged, TS6> s6,
            IProperty<INotifyPropertyChanged, TS7> s7,
            IProperty<INotifyPropertyChanged, TS8> s8,
            IProperty<INotifyPropertyChanged, TS9> s9,
            Func<TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TTarget> toTarget,
            IProperty<NotifyPropertyChanged, TTarget> target)
        {
            return new CalculatedProperty<TTarget>(
                target, t => MultiBinding.Create(s1, s2, s3, s4, s5, s6, s7, s8, s9, toTarget, t));
        }

        /// <summary>Creates and returns a <see cref="CalculatedProperty{T}"/> instance.</summary>
        /// <typeparam name="TS1">The type of the first source property.</typeparam>
        /// <typeparam name="TS2">The type of the second source property.</typeparam>
        /// <typeparam name="TS3">The type of the third source property.</typeparam>
        /// <typeparam name="TS4">The type of the fourth source property.</typeparam>
        /// <typeparam name="TS5">The type of the fifth source property.</typeparam>
        /// <typeparam name="TS6">The type of the sixth source property.</typeparam>
        /// <typeparam name="TS7">The type of the seventh source property.</typeparam>
        /// <typeparam name="TS8">The type of the eighth source property.</typeparam>
        /// <typeparam name="TS9">The type of the ninth source property.</typeparam>
        /// <typeparam name="TS10">The type of the tenth source property.</typeparam>
        /// <typeparam name="TTarget">The type of the target property.</typeparam>
        /// <exception cref="ArgumentNullException">At least one of the arguments is equal to <c>null</c>.</exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Requiring the derived type eliminates type mismatches.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Longer names would lead to unwieldy code.")]
        public static CalculatedProperty<TTarget> Create<TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10, TTarget>(
            IProperty<INotifyPropertyChanged, TS1> s1,
            IProperty<INotifyPropertyChanged, TS2> s2,
            IProperty<INotifyPropertyChanged, TS3> s3,
            IProperty<INotifyPropertyChanged, TS4> s4,
            IProperty<INotifyPropertyChanged, TS5> s5,
            IProperty<INotifyPropertyChanged, TS6> s6,
            IProperty<INotifyPropertyChanged, TS7> s7,
            IProperty<INotifyPropertyChanged, TS8> s8,
            IProperty<INotifyPropertyChanged, TS9> s9,
            IProperty<INotifyPropertyChanged, TS10> s10,
            Func<TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10, TTarget> toTarget,
            IProperty<NotifyPropertyChanged, TTarget> target)
        {
            return new CalculatedProperty<TTarget>(
                target, t => MultiBinding.Create(s1, s2, s3, s4, s5, s6, s7, s8, s9, s10, toTarget, t));
        }

        /// <summary>Creates and returns a <see cref="CalculatedProperty{T}"/> instance.</summary>
        /// <typeparam name="TS1">The type of the first source property.</typeparam>
        /// <typeparam name="TS2">The type of the second source property.</typeparam>
        /// <typeparam name="TS3">The type of the third source property.</typeparam>
        /// <typeparam name="TS4">The type of the fourth source property.</typeparam>
        /// <typeparam name="TS5">The type of the fifth source property.</typeparam>
        /// <typeparam name="TS6">The type of the sixth source property.</typeparam>
        /// <typeparam name="TS7">The type of the seventh source property.</typeparam>
        /// <typeparam name="TS8">The type of the eighth source property.</typeparam>
        /// <typeparam name="TS9">The type of the ninth source property.</typeparam>
        /// <typeparam name="TS10">The type of the tenth source property.</typeparam>
        /// <typeparam name="TS11">The type of the eleventh source property.</typeparam>
        /// <typeparam name="TTarget">The type of the target property.</typeparam>
        /// <exception cref="ArgumentNullException">At least one of the arguments is equal to <c>null</c>.</exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Requiring the derived type eliminates type mismatches.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Longer names would lead to unwieldy code.")]
        public static CalculatedProperty<TTarget> Create<TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10, TS11, TTarget>(
            IProperty<INotifyPropertyChanged, TS1> s1,
            IProperty<INotifyPropertyChanged, TS2> s2,
            IProperty<INotifyPropertyChanged, TS3> s3,
            IProperty<INotifyPropertyChanged, TS4> s4,
            IProperty<INotifyPropertyChanged, TS5> s5,
            IProperty<INotifyPropertyChanged, TS6> s6,
            IProperty<INotifyPropertyChanged, TS7> s7,
            IProperty<INotifyPropertyChanged, TS8> s8,
            IProperty<INotifyPropertyChanged, TS9> s9,
            IProperty<INotifyPropertyChanged, TS10> s10,
            IProperty<INotifyPropertyChanged, TS11> s11,
            Func<TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10, TS11, TTarget> toTarget,
            IProperty<NotifyPropertyChanged, TTarget> target)
        {
            return new CalculatedProperty<TTarget>(
                target, t => MultiBinding.Create(s1, s2, s3, s4, s5, s6, s7, s8, s9, s10, s11, toTarget, t));
        }

        /// <summary>Creates and returns a <see cref="CalculatedProperty{T}"/> instance.</summary>
        /// <typeparam name="TSource">The type of the source properties.</typeparam>
        /// <typeparam name="TTarget">The type of the target property.</typeparam>
        /// <exception cref="ArgumentNullException">At least one of the arguments is equal to <c>null</c>.</exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Requiring the derived type eliminates type mismatches.")]
        public static CalculatedProperty<TTarget> Create<TSource, TTarget>(
            IEnumerable<IProperty<INotifyPropertyChanged, TSource>> sources,
            Func<IEnumerable<TSource>, TTarget> toTarget,
            IProperty<NotifyPropertyChanged, TTarget> target)
        {
            return new CalculatedProperty<TTarget>(target, t => MultiBinding.Create(sources, toTarget, t));
        }
    }
}
