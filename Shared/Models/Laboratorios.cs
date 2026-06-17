using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Shared.Models
{
    public partial class Laboratorios: ObservableObject
    {
        public int ID { get; set; }

        [ObservableProperty]
        [JsonPropertyName("nombrelaboratorio")]

        private string direccionLora;

        [JsonPropertyName("nombrelaboratorio")]
        [ObservableProperty]
        private string nombreLaboratorio;

        [ObservableProperty]
        [JsonPropertyName("nombrelaboratorio")]

        private int edificio;

        [JsonPropertyName("nombrelaboratorio")]

        [ObservableProperty]
        private EstadoLaboratorio estatus;

        // RELACIÓN: Un laboratorio tiene asignados muchos encargados
        public ICollection<Prestamos> prestamos { get; set; } = new List<Prestamos>();
        

    }
}
