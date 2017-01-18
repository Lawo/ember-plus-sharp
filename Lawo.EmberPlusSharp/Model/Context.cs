////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    internal sealed class Context
    {
        internal Context(IParent parent, int number, string identifier)
            : this(parent, number, identifier, ChildrenRetrievalPolicy.All)
        {
        }

        internal Context(IParent parent, int number, string identifier, ChildrenRetrievalPolicy childrenRetrievalPolicy)
        {
            this.Parent = parent;
            this.Number = number;
            this.Identifier = identifier;
            this.ChildrenRetrievalPolicy = childrenRetrievalPolicy;
        }

        internal IParent Parent { get; }

        internal int Number { get; }

        internal string Identifier { get; }

        internal ChildrenRetrievalPolicy ChildrenRetrievalPolicy { get; }
    }
}
