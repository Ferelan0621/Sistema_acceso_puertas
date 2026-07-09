using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Escritorio.Data;
using Escritorio.Windows;
using Shared.Models;
using Shared.Services;

namespace Escritorio.ViewModel
{
	public partial class PeticionesViewModel : ObservableObject
	{
		private bool _mqttIniciado = false; // <-- VARIABLE DE CONTROL
		private readonly EscritorioMQTT _miBroker = SharedData.Instance.Broker;
		private readonly ApiService _apiService = new ApiService();

		public ObservableCollection<Laboratorios> ListaLaboratorios => SharedData.Instance.ListaLaboratorios;
		public ObservableCollection<PeticionMovil> ListaPeticiones { get; set; }

		public PeticionesViewModel()
		{
			ListaPeticiones = new ObservableCollection<PeticionMovil>();
			// Ya no llamamos a InicializarMQTT() aquí si quieres control total
		}

		public async Task IniciarTodo()
		{
			if (_mqttIniciado) return; // Si ya corrió, no hagas nada

			InicializarMQTT();
			_mqttIniciado = true;
		}

		private async void InicializarMQTT()
		{
			_miBroker.MensajeRecibido += MqttClient_MensajeRecibido;
			try
			{
				await _miBroker.ConectarAsync();
				await _miBroker.SuscribirseAsync(MqttServices.conexion);
				await _miBroker.SuscribirseAsync(MqttServices.cerrado);
				await _miBroker.SuscribirseAsync(MqttServices.conexion); // <-- AGREGADO

				System.Diagnostics.Debug.WriteLine($"[PETICIONES] Suscrito a: {MqttServices.conexion}, {MqttServices.cerrado} y {MqttServices.peticion}");
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"[PETICIONES] Error MQTT: {ex.Message}");
				MessageBox.Show($"Error al conectar al broker MQTT: {ex.Message}");
			}
		}

		private void MqttClient_MensajeRecibido(string topic, string payload)
		{
			System.Diagnostics.Debug.WriteLine($"[PETICIONES] Topic: '{topic}' | Payload: '{payload}'");

			Application.Current.Dispatcher.Invoke(() =>
			{
				try
				{
					var opcionesJson = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

					// 1. Petición de cierre remoto directo
					if (topic == MqttServices.cerrado)
					{
						System.Diagnostics.Debug.WriteLine("[CIERRE REMOTO] Mensaje recibido");
						var cierreRemoto = JsonSerializer.Deserialize<Prestamos>(payload, opcionesJson);

						if (cierreRemoto != null)
						{
							var resultado = MessageBox.Show(
								$"Petición de cierre recibida.\nUsuario: {cierreRemoto.UsuarioID}\nLaboratorio: {cierreRemoto.LaboratorioID}\n\n¿Cerrar el laboratorio?",
								"Cierre de Laboratorio",
								MessageBoxButton.YesNo,
								MessageBoxImage.Question);

							if (resultado == MessageBoxResult.Yes)
								_ = ProcesarCierreRemotoAsync(cierreRemoto.UsuarioID, cierreRemoto.LaboratorioID, cierreRemoto.FechaCierreRemoto);
						}
					}
					// 2. Petición de acceso o cierre por estatus desde la app móvil
					else if (topic == MqttServices.conexion) // <-- CORREGIDO A PETICION
					{
						var nuevaPeticion = JsonSerializer.Deserialize<PeticionMovil>(payload, opcionesJson);

						if (nuevaPeticion != null)
						{
							System.Diagnostics.Debug.WriteLine($"[PETICIONES] Estatus: {nuevaPeticion.Estatus}");

							if (!string.IsNullOrEmpty(nuevaPeticion.Estatus) && nuevaPeticion.Estatus.ToLower() == "cierre")
							{
								System.Diagnostics.Debug.WriteLine("[PETICIONES] Peticion de cierre recibida");
								_ = ProcesarCierreAsync(nuevaPeticion);
							}
							else
							{
								ListaPeticiones.Add(nuevaPeticion);
								System.Diagnostics.Debug.WriteLine($"[PETICIONES] Peticion agregada. Total: {ListaPeticiones.Count}");

								var lab = ListaLaboratorios.FirstOrDefault(l => l.ID == nuevaPeticion.LaboratorioID);
								if (lab != null)
								{
									lab.DatosPuerta.UsuarioNombre = $"ID: {nuevaPeticion.UsuarioID}";
									lab.DatosPuerta.Cargo = "Pendiente...";
									lab.DatosPuerta.HoraInicio = nuevaPeticion.FechaPrestamo;
									lab.OnPropertyChanged(nameof(lab.DatosPuerta));
								}

								var vm = new PeticionDialogoViewModel(nuevaPeticion, this);

								var ventana = new PeticionDialogoWindow
								{
									DataContext = vm,
									WindowStartupLocation = WindowStartupLocation.CenterScreen,
									Topmost = true
								};

								ventana.Show();
							}
						}
					}
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine($"[PETICIONES] Error: {ex.Message}");
				}
			});
		}

		private async Task<Prestamos> BuscarPrestamoActivoAsync(int laboratorioID)
		{
			var historial = await _apiService.ObtenerHistorialPrestamosAsync();
			return historial?
				.Where(p => p.LaboratorioID == laboratorioID
					&& p.FechaCierre == default(DateTime)
					&& p.FechaCierreRemoto == default(DateTime))
				.OrderByDescending(p => p.FechaSolicitud)
				.FirstOrDefault();
		}

		private async Task ProcesarCierreRemotoAsync(int usuarioID, int laboratorioID, DateTime fechaCierreRemoto)
		{
			try
			{
				var prestamo = await BuscarPrestamoActivoAsync(laboratorioID);
				if (prestamo != null)
				{
					await _apiService.CerrarPrestamoAsync(prestamo.ID, fechaCierreRemoto);
					System.Diagnostics.Debug.WriteLine($"[CIERRE REMOTO] Prestamo {prestamo.ID} cerrado en BD");
				}
				else
				{
					System.Diagnostics.Debug.WriteLine($"[CIERRE REMOTO] No se encontro prestamo activo para Lab {laboratorioID}");
				}

				var lab = ListaLaboratorios.FirstOrDefault(l => l.ID == laboratorioID);
				if (lab != null)
				{
					lab.Estatus = EstadoLaboratorio.Disponible;
					var exito = await _apiService.ActualizarLaboratorioAsync(lab);
					System.Diagnostics.Debug.WriteLine($"[CIERRE REMOTO] Lab actualizado a Disponible: {exito}");
				}
				else
				{
					System.Diagnostics.Debug.WriteLine($"[CIERRE REMOTO] ⚠️ Lab {laboratorioID} no encontrado en lista");
				}

				var cierrePayload = new
				{
					estatus = "cerrado",
					iddellaboratorio = laboratorioID,
					mensaje = "Laboratorio cerrado remotamente desde escritorio"
				};

				string jsonCierre = JsonSerializer.Serialize(cierrePayload);
				string topicoDestino = $"{MqttServices.respuesta}/{usuarioID}";

				await _miBroker.PublicarMensajeAsync(topicoDestino, jsonCierre);
				System.Diagnostics.Debug.WriteLine($"[CIERRE REMOTO] Aviso enviado a {topicoDestino}");

				await ActualizarCardsDesdeHistorialAsync();
				System.Diagnostics.Debug.WriteLine($"[CIERRE REMOTO] Card actualizada para Lab {laboratorioID}");
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"[CIERRE REMOTO] Error: {ex.Message}");
			}
		}

		private async Task ProcesarCierreAsync(PeticionMovil peticion)
		{
			try
			{
				var prestamo = await BuscarPrestamoActivoAsync(peticion.LaboratorioID);
				if (prestamo != null)
				{
					await _apiService.CerrarPrestamoAsync(prestamo.ID, DateTime.Now);
					System.Diagnostics.Debug.WriteLine($"[CIERRE] Prestamo {prestamo.ID} cerrado en BD");
				}

				var cierrePayload = new
				{
					estatus = "cerrado",
					laboratorioID = peticion.LaboratorioID,
					mensaje = "Laboratorio cerrado correctamente"
				};

				string jsonCierre = JsonSerializer.Serialize(cierrePayload);
				string topicoDestino = $"{MqttServices.respuesta}/{peticion.UsuarioID}";
				await _miBroker.PublicarMensajeAsync(topicoDestino, jsonCierre);

				await ActualizarCardsDesdeHistorialAsync();
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"[CIERRE] Error: {ex.Message}");
			}
		}

		[RelayCommand]
		private async void AceptarPeticion(PeticionMovil peticion)
		{
			try
			{
				var usuario = await _apiService.ObtenerUsuarioPorIdAsync(peticion.UsuarioID);
				var lab = ListaLaboratorios.FirstOrDefault(l => l.ID == peticion.LaboratorioID);

				System.Diagnostics.Debug.WriteLine($"[ACEPTAR] Buscando lab ID: {peticion.LaboratorioID}");
				System.Diagnostics.Debug.WriteLine($"[ACEPTAR] Lab encontrado: {(lab != null ? lab.ID.ToString() : "NULL")}");

				if (lab != null)
				{
					lab.Estatus = EstadoLaboratorio.Ocupado;
					var exito = await _apiService.ActualizarLaboratorioAsync(lab);
					System.Diagnostics.Debug.WriteLine($"[ACEPTAR] Lab actualizado en BD: {exito}");

					if (!exito) throw new Exception("No se pudo actualizar el laboratorio.");
				}

				var nuevoPrestamo = new Prestamos
				{
					UsuarioID = peticion.UsuarioID,
					LaboratorioID = peticion.LaboratorioID,
					FechaSolicitud = DateTime.Now,
					FechaApertura = DateTime.Now,
					EncargadoID = 1
				};
				var prestamoGuardado = await _apiService.GuardarPrestamoAsync(nuevoPrestamo);
				System.Diagnostics.Debug.WriteLine($"[ACEPTAR] Prestamo guardado ID: {prestamoGuardado?.ID}");

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

				await ActualizarCardsDesdeHistorialAsync();

				MessageBox.Show($"Acceso ACEPTADO al Lab {peticion.LaboratorioID} para {usuario?.Nombre ?? "Usuario #" + peticion.UsuarioID}", "Aprobado");
				ListaPeticiones.Remove(peticion);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error al aceptar peticion: {ex.Message}");
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
					laboratorioID = peticion.LaboratorioID,
					mensaje = "Acceso denegado"
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

				MessageBox.Show($"Acceso DENEGADO al Lab {peticion.LaboratorioID} al Usuario {peticion.UsuarioID}.", "Rechazado");
				ListaPeticiones.Remove(peticion);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error al denegar peticion: {ex.Message}");
			}
		}

		private async Task ActualizarCardsDesdeHistorialAsync()
		{
			try
			{
				var historial = await _apiService.ObtenerHistorialPrestamosAsync();

				Application.Current.Dispatcher.Invoke(() =>
				{
					foreach (var lab in ListaLaboratorios)
					{
						var prestamo = historial?
							.Where(p => p.LaboratorioID == lab.ID
								&& p.FechaCierre == default(DateTime)
								&& p.FechaCierreRemoto == default(DateTime))
							.OrderByDescending(p => p.FechaSolicitud)
							.FirstOrDefault();

						if (prestamo != null)
						{
							lab.DatosPuerta.UsuarioNombre = prestamo.Usuario?.Nombre ?? $"Usuario #{prestamo.UsuarioID}";
							lab.DatosPuerta.Cargo = prestamo.Usuario != null ? prestamo.Usuario.Rol.ToString() : "Sin asignar";
							lab.DatosPuerta.HoraInicio = prestamo.FechaSolicitud.ToString("dd/MM/yyyy HH:mm");
						}
						else
						{
							lab.DatosPuerta.UsuarioNombre = string.Empty;
							lab.DatosPuerta.Cargo = string.Empty;
							lab.DatosPuerta.HoraInicio = string.Empty;
							lab.DatosPuerta.EstadoPuerta = "Cerrado";
						}

						lab.OnPropertyChanged(nameof(lab.DatosPuerta));
					}
				});
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"[HISTORIAL] Error: {ex.Message}");
			}
		}
	}
}