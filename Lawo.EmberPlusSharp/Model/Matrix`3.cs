////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    /// <summary>Represents a matrix in the object tree accessible through
    /// <see cref="Consumer{T}.Root">Consumer&lt;TRoot&gt;.Root</see>.</summary>
    /// <typeparam name="TTarget">The type of the node containing the parameters of a single target.</typeparam>
    /// <typeparam name="TSource">The type of the node containing the parameters of a single source.</typeparam>
    /// <typeparam name="TConnection">The type of the node containing the parameters of a single connection.</typeparam>
    /// <threadsafety static="true" instance="false"/>
    public sealed class Matrix<TTarget, TSource, TConnection> :
        MatrixBase<Matrix<TTarget, TSource, TConnection>>
        where TTarget : Node<TTarget>
        where TSource : Node<TSource>
        where TConnection : Node<TConnection>
    {
        /// <inheritdoc cref="IMatrix.Parameters"/>
        public MatrixParameters<TTarget, TSource, TConnection> Parameters
        {
            get { return this.parameters; }
            private set { this.SetValue(ref this.parameters, value); }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal sealed override INode GetParameters() => this.Parameters;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private MatrixParameters<TTarget, TSource, TConnection> parameters;

        private Matrix()
        {
        }
    }
}
