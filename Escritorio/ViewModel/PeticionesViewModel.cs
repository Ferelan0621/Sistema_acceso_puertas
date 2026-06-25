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
			_miBroker.MensajeRecibido += MqttClient_MensajeRecibido;

			try
			{
				await _miBroker.ConectarAsync();
				await _miBroker.SuscribirseAsync(MqttServices.conexion);
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
					if (topic == MqttServices.conexion)
					{
						var opcionesJson = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
						var nuevaPeticion = JsonSerializer.Deserialize<PeticionMovil>(payload, opcionesJson);

						if (nuevaPeticion != null)
						{
							ListaPeticiones.Add(nuevaPeticion);

							// Mostramos datos provisionales en la card mientras llega la respuesta
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
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine($"Error al procesar JSON: {ex.Message}");
				}
			});
		}

		[RelayCommand]
		private async void AceptarPeticion(PeticionMovil peticion)
		{
			if (peticion == null) return;

			try
			{
				// 🔑 Consulta nombre y rol real del usuario en la API
				var usuario = await _apiService.ObtenerUsuarioPorIdAsync(peticion.UsuarioID);

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

				// 🔑 Actualiza la card con nombre y rol real
				var lab = ListaLaboratorios.FirstOrDefault(l => l.ID == peticion.LaboratorioID);
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

				// Limpia la card al denegar
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