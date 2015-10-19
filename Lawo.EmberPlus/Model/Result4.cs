////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>Represents a function result with 4 components.</summary>
    /// <typeparam name="T1">The type of the first component.</typeparam>
    /// <typeparam name="T2">The type of the second component.</typeparam>
    /// <typeparam name="T3">The type of the third component.</typeparam>
    /// <typeparam name="T4">The type of the fourth component.</typeparam>
    /// <threadsafety static="true" instance="false"/>
    [SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes", Justification = "There's no other way.")]
    public sealed class Result<T1, T2, T3, T4> : ResultBase<Result<T1, T2, T3, T4>>
    {
        private readonly ValueReader<T1> component1Reader;
        private readonly ValueReader<T2> component2Reader;
        private readonly ValueReader<T3> component3Reader;
        private readonly ValueReader<T4> component4Reader;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Initializes a new instance of the <see cref="Result{T1,T2,T3,T4}"/> class.</summary>
        public Result() : this(
            new ValueReader<T1>(), new ValueReader<T2>(), new ValueReader<T3>(), new ValueReader<T4>())
        {
        }

        /// <summary>Gets the value of the first component.</summary>
        public T1 Item1
        {
            get { return this.component1Reader.Value; }
        }

        /// <summary>Gets the value of the second component.</summary>
        public T2 Item2
        {
            get { return this.component2Reader.Value; }
        }

        /// <summary>Gets the value of the third component.</summary>
        public T3 Item3
        {
            get { return this.component3Reader.Value; }
        }

        /// <summary>Gets the value of the fourth component.</summary>
        public T4 Item4
        {
            get { return this.component4Reader.Value; }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private Result(
            ValueReader<T1> item1Reader,
            ValueReader<T2> item2Reader,
            ValueReader<T3> item3Reader,
            ValueReader<T4> item4Reader) :
            base(item1Reader, item2Reader, item3Reader, item4Reader)
        {
            this.component1Reader = item1Reader;
            this.component2Reader = item2Reader;
            this.component3Reader = item3Reader;
            this.component4Reader = item4Reader;
        }
    }
}
