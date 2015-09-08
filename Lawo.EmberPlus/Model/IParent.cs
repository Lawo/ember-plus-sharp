////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlus.Model
{
    using System.ComponentModel;
    using System.Text;

    internal interface IParent
    {
        int[] NumberPath { get; }

        void SetHasChanges();

        string GetPath();

        void AppendPath(StringBuilder builder);

        void OnPropertyChanged(PropertyChangedEventArgs e);
    }
}
