using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public static class SesionGlobal
    {
        // Aquí guardamos los datos del usuario logueado
        public static int UsuarioID { get; set; }
        public static string Nombre { get; set; }
        public static string ClaveISSEMYM { get; set; }

        // Método opcional para limpiar la sesión cuando cierre sesión
        public static void CerrarSesion()
        {
            UsuarioID = 0;
            Nombre = string.Empty;
            ClaveISSEMYM = string.Empty;
        }
    }
}
