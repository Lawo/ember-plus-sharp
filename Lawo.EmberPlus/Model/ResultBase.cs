﻿////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;

    using Ember;
    using Glow;

    /// <summary>Implements common functionality of all function results.</summary>
    /// <typeparam name="TMostDerived">The most-derived subtype of this class.</typeparam>
    /// <threadsafety static="true" instance="false"/>
    public abstract class ResultBase<TMostDerived> : IInvocationResult where TMostDerived : ResultBase<TMostDerived>
    {
        private readonly IValueReader[] valueReaders;
        private readonly TaskCompletionSource<TMostDerived> taskCompletionSource =
            new TaskCompletionSource<TMostDerived>();

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets the items in the result, see <see cref="IResult.Items"/>.</summary>
        public IEnumerable<object> Items
        {
            get { return this.valueReaders.Select(r => r.Value); }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        void IInvocationResult.Read(EmberReader reader, bool success)
        {
            this.ReadResult(reader);

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

        internal Task<TMostDerived> Task
        {
            get { return this.taskCompletionSource.Task; }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

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
