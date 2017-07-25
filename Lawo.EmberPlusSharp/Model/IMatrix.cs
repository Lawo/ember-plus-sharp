////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>Provides the common interface for all matrices in the object tree accessible through
    /// <see cref="Consumer{T}.Root">Consumer&lt;TRoot&gt;.Root</see>.</summary>
    /// <remarks>Contrary to what might be expected from the
    /// <i>"Ember+ Specification"</i><cite>Ember+ Specification</cite>, the <see cref="MaximumTotalConnects"/>,
    /// <see cref="MaximumConnectsPerTarget"/>, <see cref="Targets"/> and <see cref="Sources"/> properties offer
    /// sensible values for <b>all</b> matrix types and addressing modes. Software therefore never needs to consider the
    /// values of the redundant <i>type</i> and <i>addressingMode</i> fields, which is why they are not available as
    /// properties.</remarks>
    public interface IMatrix : INode
    {
        /// <summary>Gets <b>maximumTotalConnects</b>.</summary>
        /// <remarks>Is never 0, contains the correct number for all matrix types.</remarks>
        int MaximumTotalConnects { get; }

        /// <summary>Gets <b>maximumConnectsPerTarget</b>.</summary>
        /// <remarks>Is never 0, contains the correct number for all matrix types.</remarks>
        int MaximumConnectsPerTarget { get; }

        /// <summary>Gets the number path of the parameters associated with the matrix.</summary>
        /// <remarks>Is <c>null</c> if the provider does not send the <i>parametersLocation</i> field.</remarks>
        IReadOnlyList<int> ParametersLocation { get; }

        /// <summary>Gets <b>gainParameterNumber</b>.</summary>
        /// <remarks>Is <c>null</c> if the provider does not send the <i>gainParameterNumber</i> field.</remarks>
        int? GainParameterNumber { get; }

        /// <summary>Gets <b>labels</b></summary>
        /// <remarks>Is <c>null</c> if the provider does not send the <i>labels</i> field.</remarks>
        IReadOnlyList<MatrixLabel> Labels { get; }

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
