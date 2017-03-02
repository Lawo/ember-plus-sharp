////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>Represents the parameters of a matrix.</summary>
    /// <typeparam name="TTarget">The type of the node containing the parameters of a single target.</typeparam>
    /// <typeparam name="TSource">The type of the node containing the parameters of a single source.</typeparam>
    /// <typeparam name="TConnection">The type of the node containing the parameters of a single connection.</typeparam>
    /// <threadsafety static="true" instance="false"/>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance", Justification = "Fewer levels of inheritance would lead to more code duplication.")]
    public sealed class MatrixParameters<TTarget, TSource, TConnection> :
        FieldNode<MatrixParameters<TTarget, TSource, TConnection>>
        where TTarget : INode
        where TSource : INode
        where TConnection : INode
    {
        /// <summary>Gets <b>targets</b>.</summary>
        [Element(Identifier = "targets")]
        public CollectionNode<TTarget> Targets { get; private set; }

        /// <summary>Gets <b>sources</b>.</summary>
        [Element(Identifier = "sources")]
        public CollectionNode<TSource> Sources { get; private set; }

        /// <summary>Gets <b>connections</b>.</summary>
        [Element(Identifier = "connections")]
        public CollectionNode<CollectionNode<TConnection>> Connections { get; private set; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private MatrixParameters()
        {
        }
    }
}
