using System;
using System.Globalization;
using System.Windows.Data;

namespace AsterixViewer.Converters
{
    internal class CountToHeightConverter : IValueConverter
    {
        // Ajusta la altura del ListBox según número de elementos:
        // - mínimo: 1 línea
        // - máximo visible sin scroll: 3 líneas
        // - devuelve altura en píxeles
        // Ajusta constantes si la plantilla de item cambia.
        private const double ItemHeight = 20.0; // altura estimada por línea (px)
        private const double VerticalPadding = 8.0; // padding + borders aproximados

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                int count = 0;
                if (value is int i) count = i;
                else if (value != null && int.TryParse(value.ToString(), out int parsed)) count = parsed;

                if (count < 1) count = 1;

                int visible = Math.Min(count, 3); // mostrar hasta 3 líneas antes de scrollear
                double height = visible * ItemHeight + VerticalPadding;

                return height;
            }
            catch
            {
                // fallback razonable
                return ItemHeight + VerticalPadding;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}