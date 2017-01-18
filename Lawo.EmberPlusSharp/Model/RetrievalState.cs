////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System;

    /// <summary>Represents the state of an element with regards to whether a request needs to be sent to the provider
    /// and whether all expected information has been received.</summary>
    /// <remarks>The state of an element is always propagated up the chain of its direct and indirect parents such that
    /// a node always assumes the state of the child with the lowest state.</remarks>
    internal struct RetrievalState : IEquatable<RetrievalState>
    {
        /// <summary>Returns the lowest state of <paramref name="left"/> and <paramref name="right"/>.</summary>
        /// <remarks>Note that this is a generalization of the boolean &amp; operator to more than two states. Also
        /// known as <see href="http://en.wikipedia.org/wiki/Fuzzy_logic">Fuzzy Logic</see>.</remarks>
        public static RetrievalState operator &(RetrievalState left, RetrievalState right) =>
            new RetrievalState(Math.Min(left.state, right.state));

        public bool Equals(RetrievalState other) => this.state == other.state;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets the state used when a request needs to be issued for an element.</summary>
        /// <remarks>This is the lowest state.</remarks>
        internal static RetrievalState None { get; } = default(RetrievalState);

        /// <summary>Gets the state used when a request has been sent for a node.</summary>
        /// <remarks>This is the second lowest state.</remarks>
        internal static RetrievalState RequestSent { get; } = new RetrievalState(1);

        /// <summary>Gets the state used for subscribed stream parameters and nodes for which the provider has sent a
        /// response.</summary>
        /// <remarks>
        /// <para>This state is used for the following two elements:</para>
        /// <list type="bullet">
        /// <item>Parameter: When a stream subscription has been sent.</item>
        /// <item>Node: When the provider has sent an empty children collection or only children that are either
        /// complete or ones we're not interested in.</item>
        /// </list>
        /// <para>This is the second highest state.</para>
        /// </remarks>
        internal static RetrievalState Complete { get; } = new RetrievalState(2);

        /// <summary>Gets the state used when we have verified that all required children are present for a node.
        /// </summary>
        /// <remarks>This is the highest state.</remarks>
        internal static RetrievalState Verified { get; } = new RetrievalState(3);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private readonly int state;

        private RetrievalState(int state)
        {
            this.state = state;
        }
    }
}
