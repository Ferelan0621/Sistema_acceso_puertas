using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Models
{
    public class Laboratorios
    {
        public int ID { get; set; }
        public string IDSerial { get; set; } = null!;
        public string NombreLaboratorio { get; set; } = null!;
        public int Edificio { get; set; }
        public EstadoLaboratorio Estatus { get; set; }

        // RELACIÓN: Un laboratorio tiene asignados muchos encargados
        public ICollection<Prestamos> Prestamos { get; set; } = new List<Prestamos>();

    }
}
