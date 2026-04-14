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
                // De acuerdo a MainViewModel.cs: "ENCENDIDA (Llenando...)" y "Apagada (Nivel OK)"
                if (status.Contains("ENCENDIDA", StringComparison.OrdinalIgnoreCase))
                {
                    return Brushes.LightGreen;
                }
                if (status.Contains("Apagada", StringComparison.OrdinalIgnoreCase))
                {
                    return Brushes.LightGray;
                }
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
