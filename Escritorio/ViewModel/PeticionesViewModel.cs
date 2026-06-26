using System;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Escritorio.Data;
using Shared.Models;
using Shared.Services;

namespace Escritorio.ViewModel
{
	public partial class PeticionesViewModel : ObservableObject
	{
		private readonly EscritorioMQTT _miBroker = SharedData.Instance.Broker;
		private readonly ApiService _apiService = new ApiService();

		public ObservableCollection<Laboratorios> ListaLaboratorios => SharedData.Instance.ListaLaboratorios;
		public ObservableCollection<PeticionMovil> ListaPeticiones { get; set; }

		public PeticionesViewModel()
		{
			ListaPeticiones = new ObservableCollection<PeticionMovil>();
			InicializarMQTT();
		}

		private async void InicializarMQTT()
		{
			// 🔍 LOG 1: ¿Se engancha el evento?
			_miBroker.MensajeRecibido += MqttClient_MensajeRecibido;
			System.Diagnostics.Debug.WriteLine("[PETICIONES] Evento MensajeRecibido enganchado.");

			try
			{
				await _miBroker.ConectarAsync();

				// 🔍 LOG 2: ¿Cuál es el tópico exacto al que nos suscribimos?
				System.Diagnostics.Debug.WriteLine($"[PETICIONES] Suscribiéndose a tópico: '{MqttServices.conexion}'");
				await _miBroker.SuscribirseAsync(MqttServices.conexion);
				System.Diagnostics.Debug.WriteLine("[PETICIONES] Suscripción exitosa.");
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"[PETICIONES] Error MQTT: {ex.Message}");
				MessageBox.Show($"Error al conectar al broker MQTT: {ex.Message}");
			}
		}

		private void MqttClient_MensajeRecibido(string topic, string payload)
		{
			// 🔍 LOG 3: ¿Llega CUALQUIER mensaje MQTT?
			System.Diagnostics.Debug.WriteLine($"[PETICIONES] Mensaje recibido - Topic: '{topic}' | Payload: '{payload}'");

			Application.Current.Dispatcher.Invoke(() =>
			{
				try
				{
					// 🔍 LOG 4: ¿El tópico coincide con MqttServices.conexion?
					System.Diagnostics.Debug.WriteLine($"[PETICIONES] Comparando topic '{topic}' con MqttServices.conexion '{MqttServices.conexion}'");

					if (topic == MqttServices.conexion)
					{
						System.Diagnostics.Debug.WriteLine("[PETICIONES] ✅ Tópico coincide. Deserializando...");

						var opcionesJson = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
						var nuevaPeticion = JsonSerializer.Deserialize<PeticionMovil>(payload, opcionesJson);

						if (nuevaPeticion != null)
						{
							System.Diagnostics.Debug.WriteLine($"[PETICIONES] ✅ Petición - UsuarioID: {nuevaPeticion.UsuarioID}, LaboratorioID: {nuevaPeticion.LaboratorioID}, Estatus: {nuevaPeticion.Estatus}");

							// 🔑 Petición de CIERRE
							if (!string.IsNullOrEmpty(nuevaPeticion.Estatus) && nuevaPeticion.Estatus.ToLower() == "cierre")
							{
								System.Diagnostics.Debug.WriteLine("[PETICIONES] 🔒 Petición de cierre recibida");
								_ = ResponderCierreAsync(nuevaPeticion);
							}
							// 🔑 Petición de ACCESO normal
							else
							{
								ListaPeticiones.Add(nuevaPeticion);
								System.Diagnostics.Debug.WriteLine($"[PETICIONES] ✅ Agregada a lista. Total: {ListaPeticiones.Count}");

								var lab = ListaLaboratorios.FirstOrDefault(l => l.ID == nuevaPeticion.LaboratorioID);
								if (lab != null)
								{
									lab.DatosPuerta.UsuarioNombre = $"ID: {nuevaPeticion.UsuarioID}";
									lab.DatosPuerta.Cargo = "Pendiente...";
									lab.DatosPuerta.HoraInicio = nuevaPeticion.FechaPrestamo;
									lab.OnPropertyChanged(nameof(lab.DatosPuerta));
								}
							}
						}
						else
						{
							System.Diagnostics.Debug.WriteLine("[PETICIONES] ❌ Petición deserializada como null");
						}
					}
					else
					{
						System.Diagnostics.Debug.WriteLine($"[PETICIONES] ⚠️ Tópico NO coincide, ignorando.");
					}
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine($"[PETICIONES] ❌ Error: {ex.Message}");
				}
			});
		}

		// 🔑 Responde la petición de cierre de la app móvil
		private async Task ResponderCierreAsync(PeticionMovil peticion)
		{
			try
			{
				var cierrePayload = new
				{
					estatus = "cierre",
					laboratorioID = peticion.LaboratorioID,
					mensaje = "Laboratorio cerrado correctamente"
				};

				string jsonCierre = JsonSerializer.Serialize(cierrePayload);
				string topicoDestino = $"{MqttServices.respuesta}/{peticion.UsuarioID}";

				await _miBroker.PublicarMensajeAsync(topicoDestino, jsonCierre);
				System.Diagnostics.Debug.WriteLine($"[CIERRE] ✅ Respuesta de cierre enviada a {topicoDestino}");

				// Limpiar la card del laboratorio
				var lab = ListaLaboratorios.FirstOrDefault(l => l.ID == peticion.LaboratorioID);
				if (lab != null)
				{
					Application.Current.Dispatcher.Invoke(() =>
					{
						lab.DatosPuerta.UsuarioNombre = string.Empty;
						lab.DatosPuerta.Cargo = string.Empty;
						lab.DatosPuerta.HoraInicio = string.Empty;
						lab.DatosPuerta.EstadoPuerta = "Cerrado";
						lab.OnPropertyChanged(nameof(lab.DatosPuerta));
					});
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"[CIERRE] ❌ Error: {ex.Message}");
			}
		}

		[RelayCommand]
		private async void AceptarPeticion(PeticionMovil peticion)
		{
			if (peticion == null) return;
			try
			{
				var usuario = await _apiService.ObtenerUsuarioPorIdAsync(peticion.UsuarioID);

				// 🔑 Buscamos el laboratorio para incluir sus datos en la respuesta
				var lab = ListaLaboratorios.FirstOrDefault(l => l.ID == peticion.LaboratorioID);

				var respuestaPayload = new
				{
					estatus = "aceptado",
					usuarioID = peticion.UsuarioID,
					laboratorioID = peticion.LaboratorioID,
					nombreLaboratorio = lab?.NombreLaboratorio ?? $"Laboratorio {peticion.LaboratorioID}",
					direccionLora = lab?.DireccionLora ?? string.Empty,
					mensaje = "Acceso concedido"
				};

				string jsonRespuesta = JsonSerializer.Serialize(respuestaPayload);
				string topicoDestino = $"{MqttServices.respuesta}/{peticion.UsuarioID}";
				await _miBroker.PublicarMensajeAsync(topicoDestino, jsonRespuesta);

				string jsonAbrir = JsonSerializer.Serialize(new { d = peticion.LaboratorioID.ToString(), c = "abrir" });
				await _miBroker.PublicarMensajeAsync(MqttServices.abrir, jsonAbrir);

				// Actualizamos la card con los datos del usuario
				if (lab != null)
				{
					Application.Current.Dispatcher.Invoke(() =>
					{
						lab.DatosPuerta.UsuarioNombre = usuario?.Nombre ?? $"Usuario #{peticion.UsuarioID}";
						lab.DatosPuerta.Cargo = usuario != null ? usuario.Rol.ToString() : "Sin asignar";
						lab.DatosPuerta.EstadoPuerta = "Abierto";
						lab.OnPropertyChanged(nameof(lab.DatosPuerta));
					});
				}

				MessageBox.Show($"Has ACEPTADO el acceso al Lab {peticion.LaboratorioID} para {usuario?.Nombre ?? "Usuario #" + peticion.UsuarioID}", "Aprobado");
				ListaPeticiones.Remove(peticion);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error al enviar respuesta al móvil: {ex.Message}");
			}
		}

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

				var lab = ListaLaboratorios.FirstOrDefault(l => l.ID == peticion.LaboratorioID);
				if (lab != null)
				{
					Application.Current.Dispatcher.Invoke(() =>
					{
						lab.DatosPuerta.UsuarioNombre = string.Empty;
						lab.DatosPuerta.Cargo = string.Empty;
						lab.DatosPuerta.HoraInicio = string.Empty;
						lab.DatosPuerta.EstadoPuerta = "Cerrado";
						lab.OnPropertyChanged(nameof(lab.DatosPuerta));
					});
				}

				MessageBox.Show($"Has DENEGADO el acceso al Lab {peticion.LaboratorioID} al Usuario {peticion.UsuarioID}.", "Rechazado");
				ListaPeticiones.Remove(peticion);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error al enviar respuesta al móvil: {ex.Message}");
			}
		}
	}
}