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

		// Lista compartida con PeticionesViewModel
		public ObservableCollection<Laboratorios> ListaLaboratorios
			=> SharedData.Instance.ListaLaboratorios;

		[ObservableProperty]
		private string _statusMensaje = "Esperando conexión...";

		[ObservableProperty]
		private SolidColorBrush _statusColor = new SolidColorBrush(Colors.Gray);

		public LaboratorioViewModel()
		{
			_miBroker = SharedData.Instance.Broker;
			_miBroker.MensajeRecibido += ProcesarMensajeRecibido;
			_ = CargarDatosYConectarAsync();
		}

		private async Task CargarDatosYConectarAsync()
		{
			try
			{
				StatusMensaje = "Consultando BD...";

				// 1. Cargar laboratorios
				var labs = await _apiService.ObtenerLaboratoriosAsync();

				// 2. Cargar historial de préstamos
				var historial = await _apiService.ObtenerHistorialPrestamosAsync();

				if (labs != null)
				{
					foreach (var lab in labs)
					{
						if (lab.DatosPuerta == null)
							lab.DatosPuerta = new PuertaData();

						// 3. Solo préstamos activos (sin fecha de cierre)
						var ultimoPrestamo = historial?
							.Where(p => p.LaboratorioID == lab.ID
								&& p.FechaCierre == default(DateTime)
								&& p.FechaCierreRemoto == default(DateTime))
							.OrderByDescending(p => p.FechaSolicitud)
							.FirstOrDefault();

						// 4. Si existe, llenar la card con sus datos
						if (ultimoPrestamo != null)
						{
							lab.DatosPuerta.UsuarioNombre = ultimoPrestamo.Usuario?.Nombre
								?? $"Usuario #{ultimoPrestamo.UsuarioID}";

							lab.DatosPuerta.Cargo = ultimoPrestamo.Usuario != null
								? ultimoPrestamo.Usuario.Rol.ToString()
								: "Sin asignar";

							lab.DatosPuerta.HoraInicio = ultimoPrestamo.FechaSolicitud
								.ToString("dd/MM/yyyy HH:mm");
						}
					}

					// 5. Llenar la lista compartida
					Application.Current.Dispatcher.Invoke(() =>
					{
						SharedData.Instance.ListaLaboratorios.Clear();
						foreach (var lab in labs)
							SharedData.Instance.ListaLaboratorios.Add(lab);
					});
				}

				await _miBroker.ConectarAsync();
				await _miBroker.SuscribirseAsync(MqttServices.statusTopic);
				await _miBroker.SuscribirseAsync($"{MqttServices.doorTopic}/#");

				StatusColor = new SolidColorBrush(Colors.Green);
				StatusMensaje = "MQTT Conectado";
			}
			catch (Exception ex)
			{
				StatusColor = new SolidColorBrush(Colors.Red);
				StatusMensaje = $"Error de conexión: {ex.Message}";
				System.Diagnostics.Debug.WriteLine($"[LAB] Error: {ex.Message}");
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
			// Las peticiones del móvil las maneja PeticionesViewModel
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