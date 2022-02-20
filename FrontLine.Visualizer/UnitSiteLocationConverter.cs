using System;
using System.Globalization;
using System.Windows.Data;
using MapControl;
using RurouniJones.Dcs.FrontLine;

namespace FrontLine.Visualizer
{
    class UnitSiteLocationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var site = (UnitSite)value;
            return new Location(site.Center.Latitude, site.Center.Longitude);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}