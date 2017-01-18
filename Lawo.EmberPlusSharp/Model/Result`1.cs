////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    /// <summary>Represents a function result with a single component.</summary>
    /// <typeparam name="T1">The type of the only component.</typeparam>
    /// <threadsafety static="true" instance="false"/>
    public sealed class Result<T1> : ResultBase<Result<T1>>
    {
        /// <summary>Initializes a new instance of the <see cref="Result{T1}"/> class.</summary>
        public Result()
            : this(new ValueReader<T1>())
        {
        }

        /// <summary>Gets the value of the only component.</summary>
        public T1 Item1 => this.component1Reader.Value;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private readonly ValueReader<T1> component1Reader;

        private Result(ValueReader<T1> item1Reader)
            : base(item1Reader)
        {
            this.component1Reader = item1Reader;
        }
    }
}
