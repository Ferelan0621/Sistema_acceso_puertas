using System;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel; // Agregado para MVVM moderno
using CommunityToolkit.Mvvm.Input;
using Escritorio.Data;
using Shared.Models;
using Shared.Services;


namespace Escritorio.ViewModel
{
	// 1. SOLUCIÓN AL ERROR CS0260: Se agrega 'partial' y hereda de ObservableObject
	public partial class PeticionesViewModel : ObservableObject
	{
		// Usamos EXCLUSIVAMENTE el broker global de SharedData
		private readonly EscritorioMQTT _miBroker = SharedData.Instance.Broker;

		public ObservableCollection<Laboratorios> ListaLaboratorios => SharedData.Instance.ListaLaboratorios;

		public ObservableCollection<PeticionMovil> ListaPeticiones { get; set; }

		public PeticionesViewModel()
		{
			ListaPeticiones = new ObservableCollection<PeticionMovil>();
			InicializarMQTT();
		}

		private async void InicializarMQTT()
		{
			// 2. SOLUCIÓN ARQUITECTÓNICA: Nos colgamos del evento del Broker global, NO creamos uno nuevo.
			_miBroker.MensajeRecibido += MqttClient_MensajeRecibido;

			try
			{
				// Nos aseguramos de estar conectados y nos suscribimos al tópico de peticiones
				await _miBroker.ConectarAsync();
				string topicoSuscripcion = MqttServices.conexion;
				await _miBroker.SuscribirseAsync(topicoSuscripcion);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error al conectar al broker MQTT: {ex.Message}");
			}
		}

		private void MqttClient_MensajeRecibido(string topic, string payload)
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
				try
				{
					// Solo procesamos si el mensaje viene del tópico de conexión móvil
					if (topic == MqttServices.conexion)
					{
						var opcionesJson = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
						var nuevaPeticion = JsonSerializer.Deserialize<PeticionMovil>(payload, opcionesJson);

						if (nuevaPeticion != null)
						{
							ListaPeticiones.Add(nuevaPeticion);

							// Buscamos la tarjeta del lab
							var lab = ListaLaboratorios.FirstOrDefault(l => l.ID == nuevaPeticion.LaboratorioID);
							if (lab != null)
							{
								// Llenamos los campos visuales de tu Card con los datos del móvil
								// Usamos el operador ?? por si el móvil no manda Nombre o Cargo, que no se quede en blanco
								lab.DatosPuerta.UsuarioNombre = !string.IsNullOrEmpty(nuevaPeticion.NombreUsuario) ? nuevaPeticion.NombreUsuario : $"ID: {nuevaPeticion.UsuarioID}";
								lab.DatosPuerta.Cargo = !string.IsNullOrEmpty(nuevaPeticion.Cargo) ? nuevaPeticion.Cargo : "Pendiente...";
								lab.DatosPuerta.HoraInicio = nuevaPeticion.FechaPrestamo;
							}
						}

					}
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine($"Error al procesar JSON: {ex.Message}");
				}
			});
		}

		// 3. USO CORRECTO DE TOOLKIT: [RelayCommand] genera automáticamente 'AceptarPeticionCommand'
		[RelayCommand]
		private async void AceptarPeticion(PeticionMovil peticion)
		{
			if (peticion == null) return;

			try
			{
				var respuestaPayload = new
				{
					estatus = "aceptado",
					usuarioID = peticion.UsuarioID,
					laboratorioID = peticion.LaboratorioID,
					mensaje = $"Acceso concedido al Laboratorio {peticion.LaboratorioID}."
				};

				string jsonRespuesta = JsonSerializer.Serialize(respuestaPayload);

				string topicoDestino = $"{MqttServices.respuesta}/{peticion.UsuarioID}";
				await _miBroker.PublicarMensajeAsync(topicoDestino, jsonRespuesta);

				string jsonAbrir = JsonSerializer.Serialize(new { d = peticion.LaboratorioID.ToString(), c = "abrir" });
				await _miBroker.PublicarMensajeAsync(MqttServices.abrir, jsonAbrir);

				MessageBox.Show($"Has ACEPTADO el acceso al Lab {peticion.LaboratorioID} para el usuario {peticion.UsuarioID}", "Aprobado");

				var lab = ListaLaboratorios.FirstOrDefault(l => l.ID == peticion.LaboratorioID);
				if (lab != null)
				{
					// Mantenemos el nombre y la hora de inicio, pero confirmamos el cargo/estado
					lab.DatosPuerta.Cargo = "Aprobado / Adentro";
					lab.DatosPuerta.EstadoPuerta = "Abierto"; // Esto debería cambiar el ícono de tu puerta si lo tienes configurado
				}

				ListaPeticiones.Remove(peticion);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error al enviar respuesta al móvil: {ex.Message}");
			}
		}

		// También aplicamos [RelayCommand] aquí para generar 'DenegarPeticionCommand'
		[RelayCommand]
		private async void DenegarPeticion(PeticionMovil peticion)
		{
			if (peticion == null) return;

			try
			{
				var respuestaPayload = new
				{
					estatus = "denegado",
					usuarioID = peticion.UsuarioID,
					laboratorioID = peticion.LaboratorioID,
					mensaje = $"No se autorizó tu acceso al Laboratorio {peticion.LaboratorioID}."
				};

				string jsonRespuesta = JsonSerializer.Serialize(respuestaPayload);

				string topicoDestino = $"{MqttServices.respuesta}/{peticion.UsuarioID}";
				await _miBroker.PublicarMensajeAsync(topicoDestino, jsonRespuesta);

				MessageBox.Show($"Has DENEGADO el acceso al Lab {peticion.LaboratorioID} al Usuario {peticion.UsuarioID}.", "Rechazado");

				var lab = ListaLaboratorios.FirstOrDefault(l => l.ID == peticion.LaboratorioID);
				if (lab != null)
				{
					// Borramos los datos de la Card porque no lo dejamos entrar
					lab.DatosPuerta.UsuarioNombre = "Sin asignar";
					lab.DatosPuerta.Cargo = "Sin asignar";
					lab.DatosPuerta.HoraInicio = "--:--";
				}

				ListaPeticiones.Remove(peticion);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error al enviar respuesta al móvil: {ex.Message}");
			}
		}
	}
}