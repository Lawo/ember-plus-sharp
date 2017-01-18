////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System.Collections.Generic;

    internal sealed class StreamedParameterCollection :
        Dictionary<int, IEnumerable<IStreamedParameter>>, IStreamedParameterCollection
    {
        void IStreamedParameterCollection.Add(IStreamedParameter parameter)
        {
            var streamIdentifier = parameter.StreamIdentifier.GetValueOrDefault();
            IEnumerable<IStreamedParameter> group;

            if (!this.TryGetValue(streamIdentifier, out group))
            {
                group = new HashSet<IStreamedParameter>();
                this.Add(streamIdentifier, group);
            }

            ((HashSet<IStreamedParameter>)group).Add(parameter);
        }
    }
}
