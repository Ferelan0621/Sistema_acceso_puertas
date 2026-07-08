using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Shared.Models; // Para que reconozca el Enum EstadoLaboratorio

namespace Movil.Converters;

public class EstadoToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is EstadoLaboratorio estado)
        {
            return estado switch
            {
                EstadoLaboratorio.Disponible => ImageSource.FromFile("disponible.png") ,
                EstadoLaboratorio.Ocupado => ImageSource.FromFile("ocupado.png"),
                EstadoLaboratorio.Mantenimiento => ImageSource.FromFile("mantenimiento.png"),
                EstadoLaboratorio.Limpieza => ImageSource.FromFile("limpieza.png"), // Agregado el icono de limpieza
                _ => ImageSource.FromFile("disponible.png")
            };
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}