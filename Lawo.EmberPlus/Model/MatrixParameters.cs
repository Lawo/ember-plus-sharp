////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>Represents a label set of a matrix.</summary>
    /// <typeparam name="TTarget">The type of the node containing the parameters of a single target.</typeparam>
    /// <typeparam name="TSource">The type of the node containing the parameters of a single source.</typeparam>
    /// <typeparam name="TConnection">The type of the node containing the parameters of a single connection.</typeparam>
    [SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes", Justification = "There's no other way.")]
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance", Justification = "Fewer levels of inheritance would lead to more code duplication.")]
    public sealed class MatrixParameters<TTarget, TSource, TConnection> :
        FieldNode<MatrixParameters<TTarget, TSource, TConnection>>
        where TTarget : Node<TTarget>
        where TSource : Node<TSource>
        where TConnection : Node<TConnection>
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
    }
}
