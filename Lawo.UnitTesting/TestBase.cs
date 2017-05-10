////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright 2008-2012 Andreas Huber Doenni, original from http://phuse.codeplex.com.
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.UnitTesting
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Reflection;
    using System.Threading.Tasks;

    using Reflection;

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>Provides methods that facilitate writing unit tests.</summary>
    /// <threadsafety static="true" instance="false"/>
    public abstract class TestBase
    {
        /// <summary>Executes each action in <paramref name="actions"/> and checks that each of them throws an exception
        /// of type <typeparamref name="TException"/>.</summary>
        /// <typeparam name="TException">The type of exception to check for.</typeparam>
        /// <param name="actions">The actions to execute.</param>
        /// <exception cref="ArgumentNullException"><paramref name="actions"/> equals <c>null</c>.</exception>
        /// <exception cref="UnexpectedSuccessException">An action in <paramref name="actions"/> did not throw the
        /// expected exception. Note: this exception is deliberately inaccessible to client code, so that it cannot be
        /// caught.</exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "There's no clean way around a type parameter here.")]
        public static void AssertThrow<TException>(params Action[] actions)
            where TException : Exception
        {
            AssertThrowCore<TException>(actions, null);
        }

        /// <summary>Executes <paramref name="action"/> and checks that it throws an exception of type
        /// <typeparamref name="TException"/> and that <see cref="Exception.Message"/> equals
        /// <paramref name="expectedMessage"/>.</summary>
        /// <typeparam name="TException">The type of exception to check for.</typeparam>
        /// <param name="action">The action to execute.</param>
        /// <param name="expectedMessage">The expected message, pass <c>null</c> to not check the message.</param>
        /// <exception cref="UnexpectedMessageException"><see cref="Exception.Message"/> does not equal
        /// <paramref name="expectedMessage"/>.</exception>
        /// <exception cref="UnexpectedSuccessException"><paramref name="action"/> did not throw the
        /// expected exception. Note: this exception is deliberately inaccessible to client code, so that it cannot be
        /// caught.</exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "There's no clean way around a type parameter here.")]
        public static void AssertThrow<TException>(Action action, string expectedMessage)
            where TException : Exception
        {
            AssertThrowCore<TException>(new[] { action }, expectedMessage);
        }

        /// <summary>Asynchronously executes each action in <paramref name="actions"/> and checks that each of them
        /// throws an exception of type <typeparamref name="TException"/>.</summary>
        /// <typeparam name="TException">The type of exception to check for.</typeparam>
        /// <param name="actions">The actions to execute.</param>
        /// <exception cref="ArgumentNullException"><paramref name="actions"/> equals <c>null</c>.</exception>
        /// <exception cref="UnexpectedSuccessException">An action in <paramref name="actions"/> did not throw the
        /// expected exception. Note: this exception is deliberately inaccessible to client code, so that it cannot be
        /// caught.</exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "There's no clean way around a type parameter here.")]
        public static Task AssertThrowAsync<TException>(params Func<Task>[] actions)
            where TException : Exception
        {
            return AssertThrowCoreAsync<TException>(actions, null);
        }

        /// <summary>Asynchronously executes <paramref name="action"/> and checks that it throws an exception of type
        /// <typeparamref name="TException"/> and that <see cref="Exception.Message"/> equals
        /// <paramref name="expectedMessage"/>.</summary>
        /// <typeparam name="TException">The type of exception to check for.</typeparam>
        /// <param name="action">The action to execute.</param>
        /// <param name="expectedMessage">The expected message, pass <c>null</c> to not check the message.</param>
        /// <exception cref="UnexpectedMessageException"><see cref="Exception.Message"/> does not equal
        /// <paramref name="expectedMessage"/>.</exception>
        /// <exception cref="UnexpectedSuccessException"><paramref name="action"/> did not throw the
        /// expected exception. Note: this exception is deliberately inaccessible to client code, so that it cannot be
        /// caught.</exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "There's no clean way around a type parameter here.")]
        public static Task AssertThrowAsync<TException>(Func<Task> action, string expectedMessage)
            where TException : Exception
        {
            return AssertThrowCoreAsync<TException>(new[] { action }, expectedMessage);
        }

        /// <summary>Tests standard exception constructors.</summary>
        /// <typeparam name="T">The type of the exception to test.</typeparam>
        /// <exception cref="ArgumentException">A constructor of <typeparamref name="T"/> does not propagate the
        /// innerException argument correctly.</exception>
        /// <exception cref="Exception">Some of the expected methods are not present. See message for more information.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "There's no clean way around a type parameter here.")]
        public static void TestStandardExceptionConstructors<T>()
            where T : Exception, new()
        {
            T innerException = new T();
            CheckPropagation(innerException, null, null);

            string message = Guid.NewGuid().ToString();
            CheckPropagation(Activator<T>.CreateInstance(message), message, null);
            CheckPropagation(
                Activator<T>.CreateInstance<string, Exception>(message, innerException), message, innerException);
        }

        /// <summary>Asserts that the various equality testing methods return the expected results.</summary>
        /// <typeparam name="T">The type of the struct to be tested.</typeparam>
        /// <param name="obj1">A random value.</param>
        /// <param name="obj2">A value that is guaranteed to not be equal to <paramref name="obj1"/>.</param>
        /// <param name="equal">References the equality operator.</param>
        /// <param name="notEqual">References the inequality operator.</param>
        public static void TestStructEquality<T>(
            T obj1, T obj2, Func<T, T, bool> equal, Func<T, T, bool> notEqual)
            where T : struct, IEquatable<T>
        {
            if (equal == null)
            {
                throw new ArgumentNullException(nameof(equal));
            }

            if (notEqual == null)
            {
                throw new ArgumentNullException(nameof(notEqual));
            }

            AssertIsTrue(!obj1.Equals(obj2));
            AssertIsTrue(!obj1.Equals((object)obj2));
            AssertIsTrue(!obj1.Equals(new object()));
            AssertIsTrue(!equal(obj1, obj2));
            AssertIsTrue(notEqual(obj1, obj2));

            var obj3 = obj1;
            AssertIsTrue(obj1.Equals(obj3));
            AssertIsTrue(obj1.Equals((object)obj3));
            AssertIsTrue(equal(obj1, obj3));
            AssertIsTrue(!notEqual(obj1, obj3));

            foreach (var property in typeof(T).GetTypeInfo().DeclaredProperties)
            {
                AssertIsTrue(Equals(property.GetValue(obj1, null), property.GetValue(obj3, null)));
            }

            AssertIsTrue(obj1.GetHashCode() == obj3.GetHashCode());
        }

        /// <summary>Waits until <paramref name="property"/> has the value <paramref name="expected"/>.</summary>
        /// <typeparam name="TOwner">The type of the owner object.</typeparam>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <returns>The new value of the property.</returns>
        public static async Task<TProperty> WaitForChangeAsync<TOwner, TProperty>(
            IProperty<TOwner, TProperty> property, TProperty expected)
            where TOwner : INotifyPropertyChanged
        {
            Predicate<TProperty> predicate = v => GenericCompare.Equals(expected, v);

            if (predicate(property.Value))
            {
                return property.Value;
            }

            while (!predicate(await WaitForChangeAsync(property)))
            {
            }

            return property.Value;
        }

        /// <summary>Waits for <paramref name="property"/> to change its value.</summary>
        /// <typeparam name="TOwner">The type of the owner object.</typeparam>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <returns>The new value of the property.</returns>
        public static async Task<TProperty> WaitForChangeAsync<TOwner, TProperty>(IProperty<TOwner, TProperty> property)
            where TOwner : INotifyPropertyChanged
        {
            var changed = new TaskCompletionSource<TProperty>();
            PropertyChangedEventHandler handler =
                (s, e) =>
                {
                    if (e.PropertyName == property.PropertyInfo.Name)
                    {
                        changed.SetResult(property.Value);
                    }
                };

            property.Owner.PropertyChanged += handler;

            try
            {
                return await changed.Task;
            }
            finally
            {
                property.Owner.PropertyChanged -= handler;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets a random <see cref="string"/> value.</summary>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "A property is not suitable.")]
        protected static string GetRandomString() => Guid.NewGuid().ToString();

        /// <summary>Initializes a new instance of the <see cref="TestBase"/> class by calling
        /// <see cref="TestBase(int)">TestBase((int)DateTime.Now.TimeOfDay.TotalMilliseconds)</see>.</summary>
        /// <remarks>Deriving test classes should normally call this constructor.</remarks>
        protected TestBase()
            : this((int)DateTime.Now.TimeOfDay.TotalMilliseconds)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="TestBase"/> class by initializing the internal
        /// <see cref="Random"/> instance with <paramref name="seed"/>.</summary>
        /// <param name="seed">The seed to initialize the internal <see cref="Random"/> instance with.</param>
        /// <remarks>
        /// <para>Deriving test classes should only call this constructor to reproduce spurious test failures.</para>
        /// <para><paramref name="seed"/> is written to the console. In the case of a spurious failure, the seed value
        /// can be copied from the output of the failing test. The derived class constructor can then be changed to call
        /// this constructor with the seed value.</para>
        /// </remarks>
        protected TestBase(int seed)
        {
            this.random = new Random(seed);
            this.seed = seed;
        }

        /// <summary>Gets the internal <see cref="Random"/> instance that was initialized during construction.</summary>
        protected Random Random
        {
            get
            {
                if (this.seed != 0)
                {
                    // Console output is not captured during test class construction. That's why we only write it when
                    // the Random instance is fetched for the first time.
                    Debug.WriteLine("Random seed: {0}", this.seed);
                    this.seed = 0;
                }

                return this.random;
            }
        }

        /// <summary>Gets a random <see cref="bool"/> value.</summary>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "A property is not suitable.")]
        protected bool GetRandomBoolean() => this.Random.Next(2) == 1;

        /// <summary>Gets a random enum value.</summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        protected TEnum GetRandomEnum<TEnum>()
        {
            var values = (TEnum[])Enum.GetValues(typeof(TEnum));
            return values[this.Random.Next(values.Length)];
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static void AssertThrowCore<TException>(Action[] actions, string expectedMessage)
            where TException : Exception
        {
            foreach (Action action in actions ?? throw new ArgumentNullException(nameof(actions)))
            {
                if (action != null)
                {
                    try
                    {
                        action();
                        throw new UnexpectedSuccessException();
                    }
                    catch (TException ex)
                    {
                        if ((expectedMessage != null) && (expectedMessage != ex.Message))
                        {
                            var format = "Expected: {0}{1}Actual: {2}";
                            var message = string.Format(
                                CultureInfo.InvariantCulture, format, expectedMessage, Environment.NewLine, ex.Message);
                            throw new UnexpectedMessageException(message, ex);
                        }
                    }
                }
            }
        }

        private static async Task AssertThrowCoreAsync<TException>(Func<Task>[] actions, string expectedMessage)
            where TException : Exception
        {
            foreach (Func<Task> action in actions ?? throw new ArgumentNullException(nameof(actions)))
            {
                if (action != null)
                {
                    try
                    {
                        await action();
                        throw new UnexpectedSuccessException();
                    }
                    catch (TException ex)
                    {
                        if ((expectedMessage != null) && (expectedMessage != ex.Message))
                        {
                            var format = "Expected: {0}\r\nActual: {1}";
                            throw new UnexpectedMessageException(
                                string.Format(CultureInfo.InvariantCulture, format, expectedMessage, ex.Message), ex);
                        }
                    }
                }
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "We want to indicate that there's something wrong with T.")]
        private static void CheckPropagation(
            Exception exception, string expectedMessage, Exception expectedInnerException)
        {
            if ((expectedMessage != null) && !ReferenceEquals(expectedMessage, exception.Message))
            {
                throw new ArgumentException(
                    "A constructor of this type does not propagate the message argument correctly.", "T");
            }

            if (expectedInnerException != exception.InnerException)
            {
                throw new ArgumentException(
                    "A constructor of this type does not propagate the innerException argument correctly.", "T");
            }

            exception.Ignore();
            expectedInnerException.Ignore();
            expectedInnerException.Ignore();
        }

        private static void AssertIsTrue(bool value)
        {
            if (!value)
            {
                throw new ArgumentException("Struct equality methods are not implemented correctly.");
            }

            value.Ignore();
        }

        private readonly Random random;
        private int seed;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>The exception that is thrown when a test succeeds unexpectedly (as opposed to throwing an
        /// exception).</summary>
        [SuppressMessage("Microsoft.Design", "CA1064:ExceptionsShouldBePublic", Justification = "This one is intentionally private.")]
        private sealed class UnexpectedSuccessException : Exception
        {
            internal UnexpectedSuccessException()
            {
            }
        }

        /// <summary>The exception that is thrown when <see cref="Exception.Message"/> does not match the expected
        /// message.</summary>
        [SuppressMessage("Microsoft.Design", "CA1064:ExceptionsShouldBePublic", Justification = "This one is intentionally private.")]
        private sealed class UnexpectedMessageException : Exception
        {
            internal UnexpectedMessageException(string message, Exception innerException)
                : base(message, innerException)
            {
            }
        }
    }
}
