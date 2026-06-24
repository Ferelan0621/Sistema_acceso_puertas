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
        [JsonPropertyName("direccionLora")]

        private string direccionLora;

        [JsonPropertyName("nombrelaboratorio")]
        [ObservableProperty]
        private string nombreLaboratorio;

        [ObservableProperty]
        [JsonPropertyName("edificio")]

        private int edificio;

        [JsonPropertyName("estatus")]

        [ObservableProperty]
        private EstadoLaboratorio estatus;

		[ObservableProperty]
		[JsonPropertyName("datosPuerta")]
		private PuertaData datosPuerta = new PuertaData();


		// RELACIÓN: Un laboratorio tiene asignados muchos encargados
		[JsonIgnore]
        public ICollection<Prestamos> prestamos { get; set; } = new List<Prestamos>();
        

    }

	public partial class PuertaData : ObservableObject
	{
		[ObservableProperty]
		private string estadoPuerta;
	}
}
