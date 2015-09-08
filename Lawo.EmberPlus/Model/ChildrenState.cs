////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model
{
    using System;

    /// <summary>Represents the state of a node with regards to its children.</summary>
    /// <remarks>A parameter is always in the state <see cref="Complete"/>. The state of an element is always propagated
    /// up the chain of its direct and indirect parents such that a node always assumes the state of the child with the
    /// lowest state.</remarks>
    internal struct ChildrenState : IEquatable<ChildrenState>
    {
        private static readonly ChildrenState NoneField = new ChildrenState();
        private static readonly ChildrenState GetDirectorySentField = new ChildrenState(1);
        private static readonly ChildrenState CompleteField = new ChildrenState(2);
        private static readonly ChildrenState VerifiedField = new ChildrenState(3);

        private readonly int state;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Returns the lowest state of <paramref name="left"/> and <paramref name="right"/>.</summary>
        /// <remarks>Note that this is a generalization of the boolean &amp; operator to more than two states. Also
        /// known as <a href="http://en.wikipedia.org/wiki/Fuzzy_logic">Fuzzy Logic</a>.</remarks>
        public static ChildrenState operator &(ChildrenState left, ChildrenState right)
        {
            return new ChildrenState(Math.Min(left.state, right.state));
        }

        public bool Equals(ChildrenState other)
        {
            return this.state == other.state;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets the state used when a getDirectory request needs to be issued for a node.</summary>
        /// <remarks>This is the lowest state.</remarks>
        internal static ChildrenState None
        {
            get { return NoneField; }
        }

        /// <summary>Gets the state used when a getDirectory request has been sent for a node.</summary>
        /// <remarks>This is the second lowest state.</remarks>
        internal static ChildrenState GetDirectorySent
        {
            get { return GetDirectorySentField; }
        }

        /// <summary>Gets the state used when the provider has sent an empty children collection or we have received
        /// only children that are either complete or ones we're not interested in.</summary>
        /// <remarks>This is the second highest state.</remarks>
        internal static ChildrenState Complete
        {
            get { return CompleteField; }
        }

        /// <summary>Gets the state used when we have verified that all required children are present.</summary>
        /// <remarks>This is the highest state.</remarks>
        internal static ChildrenState Verified
        {
            get { return VerifiedField; }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private ChildrenState(int state)
        {
            this.state = state;
        }
    }
}
