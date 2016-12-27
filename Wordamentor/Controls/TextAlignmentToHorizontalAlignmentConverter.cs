using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Wordamentor.Controls
{
    public class TextAlignmentToHorizontalAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var ta = (TextAlignment) value;
            switch (ta)
            {
                case TextAlignment.Center:
                    return HorizontalAlignment.Center;
                case TextAlignment.Left:
                    return HorizontalAlignment.Left;
                case TextAlignment.Right:
                    return HorizontalAlignment.Right;
            }
            throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
