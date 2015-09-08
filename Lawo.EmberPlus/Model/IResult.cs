////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model
{
    using System.Collections.Generic;
    using Lawo.EmberPlus.Ember;

    /// <summary>Represents a function result.</summary>
    public interface IResult
    {
        /// <summary>Gets the items in the result.</summary>
        IEnumerable<object> Items
        {
            get;
        }
    }
}
