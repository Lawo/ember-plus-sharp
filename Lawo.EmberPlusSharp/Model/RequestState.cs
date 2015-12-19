////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com).</copyright>
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
    internal struct RequestState : IEquatable<RequestState>
    {
        private static readonly RequestState NoneField = new RequestState();
        private static readonly RequestState RequestSentField = new RequestState(1);
        private static readonly RequestState CompleteField = new RequestState(2);
        private static readonly RequestState VerifiedField = new RequestState(3);

        private readonly int state;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Returns the lowest state of <paramref name="left"/> and <paramref name="right"/>.</summary>
        /// <remarks>Note that this is a generalization of the boolean &amp; operator to more than two states. Also
        /// known as <see href="http://en.wikipedia.org/wiki/Fuzzy_logic">Fuzzy Logic</see>.</remarks>
        public static RequestState operator &(RequestState left, RequestState right)
        {
            return new RequestState(Math.Min(left.state, right.state));
        }

        public bool Equals(RequestState other)
        {
            return this.state == other.state;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets the state used when a request needs to be issued for an element.</summary>
        /// <remarks>This is the lowest state.</remarks>
        internal static RequestState None
        {
            get { return NoneField; }
        }

        /// <summary>Gets the state used when a request has been sent for a node.</summary>
        /// <remarks>This is the second lowest state.</remarks>
        internal static RequestState RequestSent
        {
            get { return RequestSentField; }
        }

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
        internal static RequestState Complete
        {
            get { return CompleteField; }
        }

        /// <summary>Gets the state used when we have verified that all required children are present for a node.
        /// </summary>
        /// <remarks>This is the highest state.</remarks>
        internal static RequestState Verified
        {
            get { return VerifiedField; }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private RequestState(int state)
        {
            this.state = state;
        }
    }
}
