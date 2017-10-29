//-----------------------------------------------------------------------
// Copyright 2014 Tobii Technology AB. All rights reserved.
//-----------------------------------------------------------------------

namespace Eyeplayer
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;
    using System.Windows.Markup;

    [ContentProperty("Items")]
    internal class EnumDisplayNameConverter : IValueConverter
    {
        private ObservableCollection<EnumDisplayNameItem> _items = new ObservableCollection<EnumDisplayNameItem>();
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Invoked from Xaml.")]
        public ObservableCollection<EnumDisplayNameItem> Items
        {
            get { return _items; }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return _items
                .Where(x => x.Value.Equals(value))
                .Select(x => x.DisplayName)
                .FirstOrDefault();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Invoked from Xaml.")]
    internal class EnumDisplayNameItem
    {
        public object Value { get; set; }
        public string DisplayName { get; set; }
    }
}
