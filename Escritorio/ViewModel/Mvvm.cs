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
using Escritorio.Data;

namespace Escritorio.Mvvm
{
	public partial class LaboratorioViewModel : ObservableObject
	{
		private readonly ApiService _apiService = new ApiService();
		private readonly EscritorioMQTT _miBroker; // Usaremos este nombre único
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
			// Un solo manejador para todo
			_miBroker.MensajeRecibido += ProcesarMensajeRecibido;

			_ = CargarDatosYConectarAsync();
		}

		private async Task CargarDatosYConectarAsync()
		{
			try
			{
				StatusMensaje = "Consultando BD...";
				var labs = await _apiService.ObtenerLaboratoriosAsync();
				if (labs != null) ListaLaboratorios = new ObservableCollection<Laboratorios>(labs);

				await _miBroker.ConectarAsync();

				// Suscripciones
				await _miBroker.SuscribirseAsync(MqttServices.statusTopic);
				await _miBroker.SuscribirseAsync($"{MqttServices.doorTopic}/#");
				await _miBroker.SuscribirseAsync("peticion/movil/conexion"); // Ajusta este tópico si es otro

				StatusColor = new SolidColorBrush(Colors.Green);
				StatusMensaje = "MQTT Conectado";
			}
			catch (Exception ex)
			{
				StatusColor = new SolidColorBrush(Colors.Red);
				StatusMensaje = "Error de conexión";
			}
		}

		// UN SOLO PUNTO DE ENTRADA PARA MENSAJES MQTT
		private void ProcesarMensajeRecibido(string topic, string payload)
		{
			if (string.IsNullOrEmpty(payload)) return;

			// 1. Manejo de Status del Gateway
			if (topic == MqttServices.statusTopic)
			{
				Application.Current.Dispatcher.Invoke(() => {
					bool online = payload.ToLower().Contains("online");
					StatusColor = new SolidColorBrush(online ? Colors.Green : Colors.Red);
					StatusMensaje = online ? "MQTT Conectado" : "MQTT Desconectado";
				});
			}
			// 2. Manejo de Sensores de Puerta
			else if (topic.StartsWith(MqttServices.doorTopic))
			{
				string idLabMqtt = topic.Split('/').Last(); // Obtiene el ID del final del tópico
				var lab = ListaLaboratorios.FirstOrDefault(l => l.DireccionLora == idLabMqtt);

				if (lab != null)
				{
					Application.Current.Dispatcher.Invoke(() => {
						bool abierta = payload.Contains("open") || payload.Contains("1") || payload.Contains("true");
						lab.DatosPuerta.EstadoPuerta = abierta ? "Abierto" : "Cerrado";
					});
				}
			}
			// 3. Manejo de Peticiones del Móvil (NUEVO DATO)
			else if (topic == "peticion/movil/conexion")
			{
				var datos = JsonSerializer.Deserialize<PeticionMovil>(payload);
				var lab = ListaLaboratorios.FirstOrDefault(l => l.ID == datos.LaboratorioID);

				if (lab != null)
				{
					Application.Current.Dispatcher.Invoke(() => {
						// OPCIÓN DE PRUEBA: Si no llegan los datos, pon algo fijo para verificar
						lab.DatosPuerta.UsuarioNombre = !string.IsNullOrEmpty(datos.NombreUsuario) ? datos.NombreUsuario : "Usuario #" + datos.UsuarioID;
						lab.DatosPuerta.Cargo = !string.IsNullOrEmpty(datos.Cargo) ? datos.Cargo : "Sin asignar";
						lab.DatosPuerta.HoraInicio = datos.FechaPrestamo;
						lab.DatosPuerta.EstadoPuerta = "Abierto";
					});
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