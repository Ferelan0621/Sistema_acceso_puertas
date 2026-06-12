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
                    EstadoLaboratorio.Disponible => "disponible.png",
                    EstadoLaboratorio.Ocupado => "ocupado.png",
                    EstadoLaboratorio.Mantenimiento => "mantenimiento.png",
                    EstadoLaboratorio.Limpieza => "limpieza.png",
                    _ => "desconocido.png"
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
