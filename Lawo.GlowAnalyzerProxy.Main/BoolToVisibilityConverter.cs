////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.GlowAnalyzerProxy.Main
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>Implements <see cref="IValueConverter"/> for <see cref="bool"/> and <see cref="Visibility"/>.</summary>
    public sealed class BoolToVisibilityConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value != null && (bool)value ? Visibility.Visible : Visibility.Collapsed;

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            value != null && (Visibility)value == Visibility.Visible;
    }
}
