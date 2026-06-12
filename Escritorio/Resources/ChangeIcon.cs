using Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Windows.Data;

namespace Escritorio.Resources
{
   public class ChangeIcon : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is EstadoLaboratorio estado)
            {
                return estado switch
                {
                    EstadoLaboratorio.Disponible => "pack://application:,,,/Images/disponible.png",
                    EstadoLaboratorio.Ocupado => "pack://application:,,,/Images/ocupado.png",
                    EstadoLaboratorio.Mantenimiento => "pack://application:,,,/Images/mantenimiento.png",
                    EstadoLaboratorio.Limpieza => "pack://application:,,,/Images/limpieza.png",
                    _ => "pack://application:,,,/Images/desconocido.png"
                };
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
