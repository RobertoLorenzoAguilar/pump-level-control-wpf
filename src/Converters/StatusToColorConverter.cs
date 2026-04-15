using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PumpControl.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                // Verificar INACTIVA primero porque "ACTIVA" es substring de "INACTIVA"
                if (status.Contains("INACTIVA", StringComparison.OrdinalIgnoreCase))
                {
                    // Azul oscuro apagado — bomba detenida
                    return new SolidColorBrush(Color.FromRgb(30, 41, 59));
                }
                if (status.Contains("ACTIVA", StringComparison.OrdinalIgnoreCase))
                {
                    // Verde oscuro esmeralda — bomba en operación
                    return new SolidColorBrush(Color.FromRgb(6, 78, 59));
                }
            }
            return new SolidColorBrush(Color.FromRgb(31, 41, 55));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

