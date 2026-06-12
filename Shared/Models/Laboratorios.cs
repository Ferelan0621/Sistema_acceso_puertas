using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Shared.Models
{
    public class Laboratorios
    {
        public int ID { get; set; }
        public string direccionLora { get; set; } = null!;
        [JsonPropertyName("nombrelaboratorio")]
        public string nombreLaboratorio { get; set; } = null!;
        public int edificio { get; set; }
        public EstadoLaboratorio estatus { get; set; }

        // RELACIÓN: Un laboratorio tiene asignados muchos encargados
        public ICollection<Prestamos> prestamos { get; set; } = new List<Prestamos>();

        public string estadoPuerta { get; set; } = "Cerrado";
        public string horaInicio { get; set; } = "--:--";
        public string horaFinal { get; set; } = "--:--";

    }
}
