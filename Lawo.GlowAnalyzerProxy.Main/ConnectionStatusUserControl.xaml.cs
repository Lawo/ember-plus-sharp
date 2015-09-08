////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.GlowAnalyzerProxy.Main
{
    using System.Windows;
    using System.Windows.Controls;

    using Lawo.Reflection;

    /// <summary>Represents a control that displays the status of a connection.</summary>
    internal sealed partial class ConnectionStatusUserControl : UserControl
    {
        private static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(ConnectionStatusUserControl));

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public ConnectionStatusUserControl()
        {
            this.InitializeComponent();
        }

        public string Header
        {
            get { return (string)this.GetValue(HeaderProperty); }
            set { this.SetValue(HeaderProperty, value); }
        }
    }
}
