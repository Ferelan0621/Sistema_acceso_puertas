using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Shared.Models
{
    public partial class SensorPuerta: ObservableObject
    {
        [ObservableProperty]
        [JsonIgnore]
        private string estadoPuerta = "Cerrado";

        [ObservableProperty]
        [JsonIgnore]
        private string horaInicio = "--:--";

        [ObservableProperty]
        [JsonIgnore]
        private string horaFinal = "--:--";

        // Asegúrate de que estos nombres coincidan con el JSON que mande tu API SSE
        [JsonPropertyName("sensor")]
        public string SensorMqtt { get; set; }

        [JsonPropertyName("estado")]
        public string Estado { get; set; } // Ej: "abierta", "cerrada", "true", "false"
    }
}
