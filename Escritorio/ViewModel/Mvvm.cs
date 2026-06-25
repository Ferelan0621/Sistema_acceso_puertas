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
					// Aseguramos que cada lab tenga DatosPuerta inicializado
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
						lab.OnPropertyChanged(nameof(lab.DatosPuerta));
					});
				}
			}
			// 3. Peticiones del Móvil — busca nombre y cargo en la API
			else if (topic == "peticion/movil/conexion")
			{
				// Lanzamos tarea async desde el handler sync
				_ = ProcesarPeticionMovilAsync(payload);
			}
		}

		// 🔑 Método async separado para poder hacer await a la API
		private async Task ProcesarPeticionMovilAsync(string payload)
		{
			try
			{
				var opcionesJson = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
				var datos = JsonSerializer.Deserialize<PeticionMovil>(payload, opcionesJson);

				if (datos == null)
				{
					System.Diagnostics.Debug.WriteLine("[MQTT] Payload deserializado como null");
					return;
				}

				System.Diagnostics.Debug.WriteLine($"[MQTT] Petición recibida - UsuarioID: {datos.UsuarioID}, LaboratorioID: {datos.LaboratorioID}");

				var lab = ListaLaboratorios.FirstOrDefault(l => l.ID == datos.LaboratorioID);

				if (lab == null)
				{
					System.Diagnostics.Debug.WriteLine($"[MQTT] No se encontró laboratorio con ID: {datos.LaboratorioID}");
					return;
				}

				// 🔑 Consulta el nombre y cargo del usuario en la API
				var usuario = await _apiService.ObtenerUsuarioPorIdAsync(datos.UsuarioID);

				
				System.Diagnostics.Debug.WriteLine($"[API] Usuario obtenido: {usuario?.Nombre ?? "null"} - {usuario?.Rol.ToString() ?? "null"}");

				Application.Current.Dispatcher.Invoke(() =>
				{
					lab.DatosPuerta.UsuarioNombre = usuario?.Nombre ?? "Usuario #" + datos.UsuarioID;
					// 🔑 Rol es un enum, lo convertimos a texto legible
					lab.DatosPuerta.Cargo = usuario != null ? usuario.Rol.ToString() : "Sin asignar";
					lab.DatosPuerta.HoraInicio = datos.FechaPrestamo;
					lab.DatosPuerta.EstadoPuerta = "Abierto";

					// Fuerza refresco del binding anidado
					lab.OnPropertyChanged(nameof(lab.DatosPuerta));
				});
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"[MQTT] Error procesando petición: {ex.Message}");
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