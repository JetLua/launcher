using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace launcher {
    public class StateConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var state = (Visibility)value;

            return state == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }


        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
