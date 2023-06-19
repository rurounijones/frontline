using System;
using System.Globalization;
using System.Windows.Data;
using Geo;
using MapControl;

namespace FrontLine.Visualizer
{
    class CoordinateLocationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var coordinate = (Coordinate)value;
            return new Location(coordinate.Latitude, coordinate.Longitude);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}