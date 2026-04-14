using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PumpControl.Converters
{
    public class LevelToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double level)
            {
                // De 0% a 15% es crítico (rojo), de 15% a 30% advertencia (naranja), mayor a eso es normal (verde)
                if (level < 15) return Brushes.Red;
                if (level < 30) return Brushes.Orange;
                return Brushes.MediumSeaGreen;
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
