////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model
{
    /// <summary>Represents a function result with a single component.</summary>
    /// <typeparam name="T1">The type of the only component.</typeparam>
    /// <threadsafety static="true" instance="false"/>
    public sealed class Result<T1> : ResultBase<Result<T1>>
    {
        private readonly ValueReader<T1> component1Reader;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Initializes a new instance of the <see cref="Result{T1}"/> class.</summary>
        public Result() : this(new ValueReader<T1>())
        {
        }

        /// <summary>Gets the value of the only component.</summary>
        public T1 Item1
        {
            get { return this.component1Reader.Value; }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private Result(ValueReader<T1> item1Reader) : base(item1Reader)
        {
            this.component1Reader = item1Reader;
        }
    }
}
