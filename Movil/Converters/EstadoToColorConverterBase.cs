using Shared.Models;

namespace Movil.Converters
{
    public class EstadoToColorConverterBase
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is EstadoLaboratorio estado)
            {
                return estado switch
                {
                    EstadoLaboratorio.Disponible => Colors.Green,
                    EstadoLaboratorio.Ocupado => Colors.Red,
                    EstadoLaboratorio.Limpieza => Colors.Orange,
                    EstadoLaboratorio.Mantenimiento => Colors.Gray,
                    _ => Colors.Transparent
                };
            }
            return Colors.Transparent;
        }
    }
}