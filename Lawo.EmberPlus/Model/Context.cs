////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model
{
    internal sealed class Context
    {
        private readonly IParent parent;
        private readonly int number;
        private readonly string identifier;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal Context(IParent parent, int number, string identifier)
        {
            this.parent = parent;
            this.number = number;
            this.identifier = identifier;
        }

        internal IParent Parent
        {
            get { return this.parent; }
        }

        internal int Number
        {
            get { return this.number; }
        }

        internal string Identifier
        {
            get { return this.identifier; }
        }
    }
}
