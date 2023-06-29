using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using MapControl;
using RurouniJones.Dcs.FrontLine;

namespace FrontLine.Visualizer
{
    class CoalitionPolygonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var polygon = (CoalitionPolygon) value;

            var borders = new List<List<Location>>() {
                polygon.Shell.Coordinates.Select(coord => new Location(coord.Latitude, coord.Longitude)).ToList()
            };

            foreach (var hole in polygon.Holes) {
                borders.Add(hole.Coordinates.Select(coord => new Location(coord.Latitude, coord.Longitude)).ToList());
            }

            return borders;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
