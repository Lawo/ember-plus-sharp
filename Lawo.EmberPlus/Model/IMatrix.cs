////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>Represents a matrix in the protocol specified in the
    /// <a href="http://ember-plus.googlecode.com/svn/trunk/documentation/Ember+%20Documentation.pdf">Ember+
    /// Specification</a>.</summary>
    /// <remarks>Members for which the provider does not define a value are equal to <c>null</c>.</remarks>
    public interface IMatrix : IElementWithSchemas
    {
        /// <summary>Gets <b>type</b>.</summary>
        MatrixType Type { get; }

        /// <summary>Gets <b>maximumTotalConnects</b>.</summary>
        int MaximumTotalConnects { get; }

        /// <summary>Gets <b>maximumConnectsPerTarget</b>.</summary>
        int MaximumConnectsPerTarget { get; }

        /// <summary>Gets the parameters associated with the matrix.</summary>
        INode Parameters { get; }

        /// <summary>Gets <b>gainParameterNumber</b>.</summary>
        int? GainParameterNumber { get; }

        /// <summary>Gets <b>labels</b></summary>
        IReadOnlyList<KeyValuePair<string, MatrixLabels>> Labels { get; }

        /// <summary>Gets <b>targets</b>.</summary>
        /// <remarks>Is never <c>null</c>, contains the target numbers for linear and nonlinear matrices.</remarks>
        IReadOnlyList<int> Targets { get; }

        /// <summary>Gets <b>sources</b>.</summary>
        /// <remarks>Is never <c>null</c>, contains the source numbers for linear and nonlinear matrices.</remarks>
        IReadOnlyList<int> Sources { get; }

        /// <summary>Gets <b>connections</b>.</summary>
        /// <remarks> Is never <c>null</c>, the value is guaranteed to contain an entry for each element in the
        /// <see cref="Targets"/> value.</remarks>
        IReadOnlyDictionary<int, ObservableCollection<int>> Connections { get; }
    }
}
