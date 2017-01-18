////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.GlowAnalyzerProxy.Main
{
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;

#pragma warning disable SA1124 // Do not use regions. Necessary so that tested code snippets can be included in the documentation.
    /// <summary>Represents the main window of the application.</summary>
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Instantiated through reflection.")]
    internal sealed partial class MainWindow
    {
        #region SetDataContext
        public MainWindow()
        {
            this.InitializeComponent();
            this.DataContext = new MainWindowViewModel(Properties.Settings.Default);
            this.ViewModel.ScrollEventIntoView += this.OnScrollEventIntoView;
            this.ViewModel.ListenFailed += this.OnListenFailed;
        }
        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private MainWindowViewModel ViewModel => (MainWindowViewModel)this.DataContext;

        private void OnScrollEventIntoView(object sender, ScrollEventIntoViewEventArgs e) =>
            this.EventsDataGrid.ScrollIntoView(e.NewEvent);

        private void OnListenFailed(object sender, ListenFailedEventArgs e)
        {
            var caption = string.Format(
                CultureInfo.InvariantCulture, "Unable to listen on port {0}", this.ViewModel.ListeningPort);
            MessageBox.Show(this, e.Exception.Message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void OnSelectLogFolderButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.RootFolder = System.Environment.SpecialFolder.Desktop;
                dialog.SelectedPath = this.ViewModel.LogFolder;

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    this.ViewModel.LogFolder = dialog.SelectedPath;
                }
            }
        }

        private void OnDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var column = (DataGridBoundColumn)e.Column;

            if ((e.PropertyType == typeof(int)) || (e.PropertyType == typeof(int?)) || (e.PropertyType == typeof(double)))
            {
                var setter = new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Right);
                column.CellStyle = new Style() { Setters = { setter } };
            }

            if ((e.PropertyType == typeof(int)) || (e.PropertyType == typeof(int?)))
            {
                column.Binding.StringFormat = "#,#";
            }
            else if (e.PropertyType == typeof(double))
            {
                column.Binding.StringFormat = "0.00s";
            }
        }
    }
#pragma warning restore SA1124 // Do not use regions
}
