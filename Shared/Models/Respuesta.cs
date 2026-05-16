using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Models
{
    public class Respuesta
    {
        public int Id { get; set; }
        public bool Aceptada { get; set; } 
        public string Nombre_Laboratorio { get; set; } = null!;

        // Clave foránea hacia la petición asociada
        public int PeticionId { get; set; }

        // Propiedad de navegación hacia la petición asociada
        public Peticiones Peticion { get; set; } = null!;

    }
}
