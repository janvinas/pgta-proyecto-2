using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AsterixViewer.Converters
{
    public class EstadoAsterixConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string estado)
            {
                switch (estado)
                {
                    case "Pendiente":
                        return Brushes.Red;
                    case "✔ Cargado":
                        return Brushes.LimeGreen;
                    case "✔ Cargada":
                        return Brushes.LimeGreen;
                    default:
                        return Brushes.Gray;
                }
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
