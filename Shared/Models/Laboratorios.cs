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
        public string direccionLora;

        [JsonPropertyName("nombrelaboratorio")]
        [ObservableProperty]
        public string nombreLaboratoriol;
        [ObservableProperty]
        public int edificio;
        [ObservableProperty]
        public EstadoLaboratorio estatus;

        // RELACIÓN: Un laboratorio tiene asignados muchos encargados
        public ICollection<Prestamos> prestamos { get; set; } = new List<Prestamos>();
        

    }
}
