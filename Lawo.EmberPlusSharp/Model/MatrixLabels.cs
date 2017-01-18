////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>Represents a label set of a matrix.</summary>
    /// <threadsafety static="true" instance="false"/>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance", Justification = "Fewer levels of inheritance would lead to more code duplication.")]
    public sealed class MatrixLabels : FieldNode<MatrixLabels>
    {
        /// <summary>Gets targets.</summary>
        [Element(Identifier = "targets")]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called through reflection.")]
        public CollectionNode<StringParameter> Targets { get; private set; }

        /// <summary>Gets sources.</summary>
        [Element(Identifier = "sources")]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called through reflection.")]
        public CollectionNode<StringParameter> Sources { get; private set; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private MatrixLabels()
        {
        }
    }
}
