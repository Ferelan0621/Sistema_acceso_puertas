using System.Collections.Generic;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Shared.Models
{
	public partial class Laboratorios : ObservableObject
	{
		public int ID { get; set; }

		[ObservableProperty]
		[JsonPropertyName("direccionLora")]
		private string _direccionLora;

		[ObservableProperty]
		[JsonPropertyName("nombrelaboratorio")]
		private string _nombreLaboratorio;

		[ObservableProperty]
		[JsonPropertyName("edificio")]
		private int _edificio;

		[ObservableProperty]
		[JsonPropertyName("estatus")]
		private EstadoLaboratorio _estatus;

		[JsonIgnore]
		public ICollection<Prestamos> Prestamos { get; set; } = new List<Prestamos>();
	}

	public partial class PuertaData : ObservableObject
	{
		[ObservableProperty]
		private string _usuarioNombre;

		[ObservableProperty]
		private string _cargo;

		[ObservableProperty]
		private string _horaInicio;

		[ObservableProperty]
		private string _horaFinal;

		[ObservableProperty]
		private string _estadoPuerta;
	}
	public partial class DatosCambio : ObservableObject
	{
        [ObservableProperty]
        [JsonPropertyName("datosPuerta")]
        private PuertaData _datosPuerta = new PuertaData();

        partial void OnDatosPuertaChanged(PuertaData oldValue, PuertaData newValue)
        {
            if (newValue == null)
                DatosPuerta = new PuertaData();
        }

        public new void OnPropertyChanged(string propertyName)
            => base.OnPropertyChanged(propertyName);
    }
}