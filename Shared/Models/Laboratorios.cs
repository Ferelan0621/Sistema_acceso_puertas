using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Models
{
    public class Laboratorios
    {
        public int Id { get; set; }
        public string Id_Serial { get; set; } = null!;
        public string Nombre_Laboratorio { get; set; } = null!;
        public int Edifico { get; set; }
        public EstadoLaboratorio Estado { get; set; }

        // RELACIÓN: Un laboratorio tiene asignados muchos encargados
        public ICollection<Prestamos> Prestamos { get; set; } = new List<Prestamos>();

    }
}
