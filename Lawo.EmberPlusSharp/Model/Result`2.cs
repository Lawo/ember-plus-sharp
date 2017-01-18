////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    /// <summary>Represents a function result with 2 components.</summary>
    /// <typeparam name="T1">The type of the first component.</typeparam>
    /// <typeparam name="T2">The type of the second component.</typeparam>
    /// <threadsafety static="true" instance="false"/>
    public sealed class Result<T1, T2> : ResultBase<Result<T1, T2>>
    {
        /// <summary>Initializes a new instance of the <see cref="Result{T1,T2}"/> class.</summary>
        public Result()
            : this(new ValueReader<T1>(), new ValueReader<T2>())
        {
        }

        /// <summary>Gets the value of the first component.</summary>
        public T1 Item1 => this.component1Reader.Value;

        /// <summary>Gets the value of the second component.</summary>
        public T2 Item2 => this.component2Reader.Value;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private readonly ValueReader<T1> component1Reader;
        private readonly ValueReader<T2> component2Reader;

        private Result(ValueReader<T1> item1Reader, ValueReader<T2> item2Reader)
            : base(item1Reader, item2Reader)
        {
            this.component1Reader = item1Reader;
            this.component2Reader = item2Reader;
        }
    }
}
