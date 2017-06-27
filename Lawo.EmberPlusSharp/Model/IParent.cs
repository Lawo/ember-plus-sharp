////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System.ComponentModel;
    using System.Text;

    internal interface IParent
    {
        int[] NumberPath { get; }

        void SetHasChanges();

        void SetIsOnline();

        void ResetRetrievalState();

        string GetPath();

        void AppendPath(StringBuilder builder);

        void OnPropertyChanged(PropertyChangedEventArgs e);
    }
}
