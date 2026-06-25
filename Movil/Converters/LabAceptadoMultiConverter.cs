using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;
using System;
using System.Globalization;
using Shared.Models;

namespace Movil.Converters
{
    public class LabAceptadoMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 3) return Colors.Transparent;

            if (!int.TryParse(values[0]?.ToString(), out int idLab)) return Colors.Transparent;
            if (!int.TryParse(values[1]?.ToString(), out int idAceptado)) return Colors.Transparent;

            if (values[2] is EstadoLaboratorio estado)
            {
                // Si no hay nada aceptado, regresa al color original según el estado
                if (idAceptado == -1)
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

                // Si este es el aceptado: Morado
                if (idLab == idAceptado) return Color.FromArgb("#8E44AD");

                // Si es otro, está inhabilitado: Gris
                return Color.FromArgb("#B0BEC5");
            }
            return Colors.Transparent;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}