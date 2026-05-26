using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Models
{
    public class Usuarios
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Clave_ISSEMYM { get; set; } = null!;
        public Rol Rol { get; set; }

        // Relación uno a muchos con Prestamos
        public ICollection<Prestamos> Prestamos { get; set; } = new List<Prestamos>();
    }
}
