////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.GlowAnalyzerProxy.Main
{
    using System.Windows;

    /// <summary>Represents a control that displays the status of a connection.</summary>
    internal sealed partial class ConnectionStatusUserControl
    {
        public ConnectionStatusUserControl()
        {
            this.InitializeComponent();
        }

        public string Header
        {
            get { return (string)this.GetValue(HeaderProperty); }
            set { this.SetValue(HeaderProperty, value); }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(ConnectionStatusUserControl));
    }
}
