    using Microsoft.Maui.Graphics; // Para los Colors
    using Microsoft.Maui.Controls; // Para IValueConverter
    using System;
    using System.Globalization;
    using Shared.Models;

    namespace Movil.Converters
    {
        // 1. AGREGA ": IValueConverter" AQUÍ
        public class EstadoToColorConverterBase : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is EstadoLaboratorio estado)
                {
                    return estado switch
                    {
                        EstadoLaboratorio.Disponible => Colors.Green,
                        EstadoLaboratorio.Ocupado => Colors.Red,
                        EstadoLaboratorio.Limpieza => Colors.LightSkyBlue,
                        EstadoLaboratorio.Mantenimiento => Colors.Orange,
                        _ => Colors.Transparent
                    };
                }
                return Colors.Transparent;
            }

            // 2. AGREGA ESTE MÉTODO OBLIGATORIO (MAUI lo exige para compilar)
            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }