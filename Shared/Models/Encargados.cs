using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Models
{
    public class Encargados
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string password { get; set; } = null!;
        public bool Activo { get; set; }
        public Rol Rol { get; set; } = Rol.Administrador;

        // VÍNCULO: ID del laboratorio al que pertenece este encargado
        public int PrestamoId { get; set; }
        public Prestamos Prestamos { get; set; } = null!;
    }
}
