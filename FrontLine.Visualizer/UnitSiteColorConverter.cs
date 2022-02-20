using System;
using System.Globalization;
using System.Windows.Data;
using RurouniJones.Dcs.FrontLine;

namespace FrontLine.Visualizer
{
    class UnitSiteColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var site = (UnitSite)value;
            return site.Coalition == CoalitionId.RedFor ? "Red" : "Blue";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
