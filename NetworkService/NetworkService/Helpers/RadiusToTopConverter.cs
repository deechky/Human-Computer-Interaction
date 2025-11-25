using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NetworkService.Helpers
{
    public class RadiusToTopConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double radius = (double)value;
            // Ako hoćeš da svi krugovi "vise" sa iste linije (X-osa gore),
            // treba pomeriti tako da donja ivica bude ista: (maxRadius - radius)
            double maxRadius = 100; // zavisi koliko dozvoliš max krug
            return maxRadius - radius;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
