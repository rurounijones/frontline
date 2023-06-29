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
            if (site.Coalition == CoalitionId.Neutral)
            {
                return "Green";
            }
            else if (site.Coalition == CoalitionId.RedFor)
            {
                return "Red";
            }
            else if (site.Coalition == CoalitionId.BlueFor)
            {
                return "Blue";
            } else
            { 
                return "Orange";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
