using System;
using System.Collections.Generic;
using System.Text;

namespace Api.Models
{
    public class Laboratorios
    {
        public int ID { get; set; }
        public string IDSerial { get; set; } = null!;
        public string Nombre_Laboratorio { get; set; } = null!;
        public int Edifico { get; set; }
        public EstadoLaboratorio Estatus { get; set; }

        // RELACIÓN: Un laboratorio tiene asignados muchos encargados
        public ICollection<Prestamos> Prestamos { get; set; } = new List<Prestamos>();

    }
}
