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
                if (idLab == idAceptado)
                    return Color.FromArgb("#9C27B0"); // Morado (Diferente a verde, rojo, azul o naranja)
                else
                    return Color.FromArgb("#E0E0E0"); // Gris (Visualmente inhabilitado)
            }
            return Colors.White;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
//public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
//    {
//        if (values == null || values.Length < 3) return Colors.Transparent;

//        if (values[0] is int id && values[1] is int labAceptadoId && values[2] is EstadoLaboratorio estatus)
//        {
//            // CASO A: Ya hay un laboratorio aceptado
//            if (labAceptadoId != -1)
//            {
//                if (id == labAceptadoId)
//                    return Color.FromArgb("#9C27B0"); // Morado (Diferente a verde, rojo, azul o naranja)
//                else
//                    return Color.FromArgb("#E0E0E0"); // Gris (Visualmente inhabilitado)
//            }

//            // CASO B: Flujo normal, nadie ha sido aceptado aún
//            return estatus switch
//            {
//                EstadoLaboratorio.Disponible => Color.FromArgb("#FFFFFF"), // Blanco/Normal
//                EstadoLaboratorio.Ocupado => Color.FromArgb("#FFEBEE"), // Ocupado
//                _ => Colors.White
//            };
//        }
//        return Colors.White;
//    }

//    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
//    {
//        throw new NotImplementedException();
//    }
//}