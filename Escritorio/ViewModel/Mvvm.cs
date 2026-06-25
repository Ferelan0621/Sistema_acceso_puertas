using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Escritorio.Data;
using Shared.Models;
using Shared.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Threading;

namespace Escritorio.Mvvm
{
	public partial class LaboratorioViewModel : ObservableObject
	{
		private readonly ApiService _apiService = new ApiService();
		private readonly EscritorioMQTT _miBroker;
		private CancellationTokenSource _sseCancellationTokenSource;

		[ObservableProperty]
		private ObservableCollection<Laboratorios> _listaLaboratorios = new();

		[ObservableProperty]
		private string _statusMensaje = "Esperando conexión...";

		[ObservableProperty]
		private SolidColorBrush _statusColor = new SolidColorBrush(Colors.Gray);

		public LaboratorioViewModel()
		{
			_miBroker = new EscritorioMQTT();
			_miBroker.MensajeRecibido += ProcesarMensajeRecibido;

			_ = CargarDatosYConectarAsync();
		}

		private async Task CargarDatosYConectarAsync()
		{
			try
			{
				StatusMensaje = "Consultando BD...";
				var labs = await _apiService.ObtenerLaboratoriosAsync();

				if (labs != null)
				{
					// 🔑 FIX 3: Aseguramos que cada lab tenga DatosPuerta inicializado
					foreach (var lab in labs)
					{
						if (lab.DatosPuerta == null)
							lab.DatosPuerta = new PuertaData();
					}
					ListaLaboratorios = new ObservableCollection<Laboratorios>(labs);
				}

				await _miBroker.ConectarAsync();

				await _miBroker.SuscribirseAsync(MqttServices.statusTopic);
				await _miBroker.SuscribirseAsync($"{MqttServices.doorTopic}/#");
				await _miBroker.SuscribirseAsync("peticion/movil/conexion");

				StatusColor = new SolidColorBrush(Colors.Green);
				StatusMensaje = "MQTT Conectado";
			}
			catch (Exception ex)
			{
				StatusColor = new SolidColorBrush(Colors.Red);
				StatusMensaje = $"Error de conexión: {ex.Message}";
			}
		}

		private void ProcesarMensajeRecibido(string topic, string payload)
		{
			if (string.IsNullOrEmpty(payload)) return;

			// 1. Status del Gateway
			if (topic == MqttServices.statusTopic)
			{
				Application.Current.Dispatcher.Invoke(() =>
				{
					bool online = payload.ToLower().Contains("online");
					StatusColor = new SolidColorBrush(online ? Colors.Green : Colors.Red);
					StatusMensaje = online ? "MQTT Conectado" : "MQTT Desconectado";
				});
			}
			// 2. Sensores de Puerta
			else if (topic.StartsWith(MqttServices.doorTopic))
			{
				string idLabMqtt = topic.Split('/').Last();
				var lab = ListaLaboratorios.FirstOrDefault(l => l.DireccionLora == idLabMqtt);

				if (lab != null)
				{
					Application.Current.Dispatcher.Invoke(() =>
					{
						bool abierta = payload.Contains("open") || payload.Contains("1") || payload.Contains("true");
						lab.DatosPuerta.EstadoPuerta = abierta ? "Abierto" : "Cerrado";

						// 🔑 FIX 4: Fuerza refresco del binding anidado
						lab.OnPropertyChanged(nameof(lab.DatosPuerta));
					});
				}
			}
			// 3. Peticiones del Móvil
			else if (topic == "peticion/movil/conexion")
			{
				try
				{
					var datos = JsonSerializer.Deserialize<PeticionMovil>(payload);
					if (datos == null) return;

					var lab = ListaLaboratorios.FirstOrDefault(l => l.ID == datos.LaboratorioID);

					if (lab != null)
					{
						Application.Current.Dispatcher.Invoke(() =>
						{
							// Actualiza las propiedades internas de DatosPuerta
							lab.DatosPuerta.UsuarioNombre = !string.IsNullOrEmpty(datos.NombreUsuario)
								? datos.NombreUsuario
								: "Usuario #" + datos.UsuarioID;

							lab.DatosPuerta.Cargo = !string.IsNullOrEmpty(datos.Cargo)
								? datos.Cargo
								: "Sin asignar";

							lab.DatosPuerta.HoraInicio = datos.FechaPrestamo;
							lab.DatosPuerta.EstadoPuerta = "Abierto";

							// 🔑 FIX 5: Fuerza que la card entera refresque DatosPuerta
							lab.OnPropertyChanged(nameof(lab.DatosPuerta));
						});
					}
				}
				catch (JsonException ex)
				{
					// Log del error para debugging
					System.Diagnostics.Debug.WriteLine($"[MQTT] Error deserializando peticion: {ex.Message}");
					System.Diagnostics.Debug.WriteLine($"[MQTT] Payload recibido: {payload}");
				}
			}
		}

		[RelayCommand]
		private async Task AbrirLabAsync(Laboratorios labSeleccionado)
		{
			if (labSeleccionado == null) return;
			string json = JsonSerializer.Serialize(new { d = labSeleccionado.ID.ToString(), c = "abrir" });
			await _miBroker.PublicarMensajeAsync(MqttServices.abrir, json);
		}

		public void DesconectarSSE() => _sseCancellationTokenSource?.Cancel();
	}
}