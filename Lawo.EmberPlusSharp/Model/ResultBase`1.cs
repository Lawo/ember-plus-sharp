////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;

    using Ember;
    using Glow;

    /// <summary>This class is not intended to be referenced in your code.</summary>
    /// <remarks>Provides the common implementation for all results of functions in the object tree accessible through
    /// <see cref="Consumer{T}.Root">Consumer&lt;TRoot&gt;.Root</see>.</remarks>
    /// <typeparam name="TMostDerived">The most-derived subtype of this class.</typeparam>
    /// <threadsafety static="true" instance="false"/>
    public abstract class ResultBase<TMostDerived> : IInvocationResult
        where TMostDerived : ResultBase<TMostDerived>
    {
        /// <inheritdoc/>
        public IEnumerable<object> Items
        {
            get { return this.valueReaders.Select(r => r.Value); }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <inheritdoc/>
        void IInvocationResult.Read(EmberReader reader) => this.ReadResult(reader);

        /// <inheritdoc/>
        void IInvocationResult.Publish(bool success)
        {
            if (success)
            {
                this.taskCompletionSource.SetResult((TMostDerived)this);
            }
            else
            {
                this.taskCompletionSource.SetException(
                    new InvocationFailedException("The function invocation failed.", this));
            }
        }

        internal ResultBase(params IValueReader[] valueReaders)
        {
            this.valueReaders = valueReaders;
        }

        internal Task<TMostDerived> Task => this.taskCompletionSource.Task;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private readonly IValueReader[] valueReaders;
        private readonly TaskCompletionSource<TMostDerived> taskCompletionSource =
            new TaskCompletionSource<TMostDerived>();

        private void ReadResult(EmberReader reader)
        {
            int index;

            for (index = 0; reader.Read() && (reader.InnerNumber != InnerNumber.EndContainer);)
            {
                if (reader.GetContextSpecificOuterNumber() == GlowTuple.Value.OuterNumber)
                {
                    if (index >= this.valueReaders.Length)
                    {
                        throw this.CreateSignatureMismatchException();
                    }

                    this.valueReaders[index].ReadValue(reader);
                    ++index;
                }
                else
                {
                    reader.Skip();
                }
            }

            if (index < this.valueReaders.Length)
            {
                throw this.CreateSignatureMismatchException();
            }
        }

        private ModelException CreateSignatureMismatchException()
        {
            const string Format = "The received tuple length does not match the tuple description length of {0}.";
            throw new ModelException(string.Format(CultureInfo.InvariantCulture, Format, this.valueReaders.Length));
        }
    }
}
